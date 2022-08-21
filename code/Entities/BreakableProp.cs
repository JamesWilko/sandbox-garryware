using Sandbox;
using System;
using System.Threading.Tasks;
using SandboxEditor;

namespace Garryware.Entities;

/// <summary>
/// A prop that physically simulates as a single rigid body. It can be constrained to other physics objects using hinges
/// or other constraints. It can also be configured to break when it takes enough damage.
/// Note that the health of the object will be overridden by the health inside the model, to ensure consistent health game-wide.
/// If the model used by the prop is configured to be used as a prop_dynamic (i.e. it should not be physically simulated) then it CANNOT be
/// used as a prop_physics. Upon level load it will display a warning in the console and remove itself. Use a prop_dynamic instead.
/// </summary>
public partial class BreakableProp : BasePhysics
{
    /// <summary>
    /// If set, the prop will spawn with motion disabled and will act as a nav blocker until broken.
    /// </summary>
    [Property]
    public bool Static { get; set; } = false;

    /// <summary>
    /// If set, the prop will spawn its associated gibs when it is destroyed.
    /// </summary>
    [Property]
    public bool CanGib { get; set; } = false;
    
    [Property("boneTransforms"), HideInEditor]
    private string BoneTransforms { get; set; }

    /// <summary>
    /// Multiplier for the object's mass.
    /// </summary>
    [Property("massscale", Title = "Mass Scale"), Category("Physics Properties")]
    private float MassScale { get; set; } = 1.0f;

    /// <summary>
    /// Physics linear damping.
    /// </summary>
    [Property("lineardamping", Title = "Linear Damping"), Category("Physics Properties")]
    private float LinearDamping { get; set; } = 0.0f;

    /// <summary>
    /// Physics angular damping.
    /// </summary>
    [Property("angulardamping", Title = "Angular Damping"), Category("Physics Properties")]
    private float AngularDamping { get; set; } = 0.0f;

    public delegate void OnBrokenDelegate(Entity attacker);
    public event OnBrokenDelegate OnBroken;
    
    public override void Spawn()
    {
        base.Spawn();

        PhysicsEnabled = true;
        UsePhysicsCollision = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;
        Tags.Add("prop", "solid");

        if (Static)
        {
            PhysicsEnabled = false;
        }
        else
        {
            SetupPhysics();
        }
        
        // @todo: check if a microgame is running and automatically add this ent to the auto-cleanup
    }

    private void SetupPhysics()
    {
        var physics = SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
        if (!physics.IsValid())
            return;

        // Apply any saved bone transforms
        ApplyBoneTransforms();

        if (MassScale != 1.0f)
        {
            physics.Mass *= MassScale;
        }

        physics.LinearDamping = LinearDamping;
        physics.AngularDamping = AngularDamping;
    }

    private void ApplyBoneTransforms()
    {
        if (string.IsNullOrWhiteSpace(BoneTransforms))
            return;

        var bones = BoneTransforms.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var bone in bones)
        {
            var split = bone.Split(':', StringSplitOptions.TrimEntries);
            if (split.Length != 2)
                continue;

            var boneName = split[0];
            var boneTransform = Transform.Parse(split[1]);

            var body = GetBonePhysicsBody(GetBoneIndex(boneName));
            if (body.IsValid())
            {
                body.Transform = Transform.ToWorld(boneTransform);
            }
        }
    }

    public override void OnNewModel(Model model)
    {
        // When a model is reloaded, all entities get set to NULL model first
        if (model.IsError) return;

        base.OnNewModel(model);

        if (IsServer)
        {
            UpdatePropData(model);
        }
    }

    protected new virtual void UpdatePropData(Model model)
    {
        Host.AssertServer();

        if (model.TryGetData(out ModelPropData propInfo))
        {
            Health = propInfo.Health;
        }

        //
        // If health is unset, set it to -1 - which means it cannot be destroyed
        //
        if (Health <= 0)
            Health = -1;
    }

    protected DamageInfo LastDamage;

    /// <summary>
    /// Fired when the entity gets damaged.
    /// </summary>
    protected Output OnDamaged { get; set; }

    /// <summary>
    /// This prop won't be able to be damaged for this amount of time
    /// </summary>
    public RealTimeUntil Invulnerable { get; set; }

    public override void TakeDamage(DamageInfo info)
    {
        if (Invulnerable > 0)
        {
            // We still want to apply forces
            ApplyDamageForces(info);

            return;
        }

        LastDamage = info;

        base.TakeDamage(info);

        // TODO: Add damage type as argument? Or should it be the new health value?
        OnDamaged.Fire(this);
    }

    public override void OnKilled()
    {
        if (LifeState != LifeState.Alive)
            return;

        LifeState = LifeState.Dead;

        if (LastDamage.Flags.HasFlag(DamageFlags.PhysicsImpact))
        {
            Velocity = lastCollision.This.PreVelocity;
        }

        if (HasExplosionBehavior())
        {
            if (LastDamage.Flags.HasFlag(DamageFlags.Blast))
            {
                LifeState = LifeState.Dying;

                // Don't explode right away and cause a stack overflow
                var rand = new Random();
                _ = ExplodeAsync(RandomExtension.Float(rand, 0.05f, 0.25f));

                return;
            }
            else
            {
                DoGibs();
                DoExplosion();
                Delete(); // LifeState.Dead prevents this in OnKilled
            }
        }
        else
        {
            DoGibs();
            Delete(); // LifeState.Dead prevents this in OnKilled
        }

        // Call an event
        OnBroken?.Invoke(LastDamage.Attacker);
        
        base.OnKilled();
    }

    CollisionEventData lastCollision;

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        lastCollision = eventData;

        base.OnPhysicsCollision(eventData);
    }

    private bool HasExplosionBehavior()
    {
        if (Model == null || Model.IsError)
            return false;

        return Model.HasData<ModelExplosionBehavior>();
    }

    /// <summary>
    /// Fired when the entity gets destroyed.
    /// </summary>
    protected Output OnBreak { get; set; }

    private void DoGibs()
    {
        if(!CanGib) return;
        
        var result = new Breakables.Result();
        result.CopyParamsFrom(LastDamage);
        Breakables.Break(this, result);

        // This applies forces from explosive damage to our gibs... But this is already done by DoExplosion, we just need to make sure its called after spawning gibs.
        /*if ( LastDamage.Flags.HasFlag( DamageFlags.Blast ) )
        {
            foreach ( var prop in result.Props )
            {
                if ( !prop.IsValid() )
                    continue;

                var body = prop.PhysicsBody;
                if ( !body.IsValid() )
                    continue;

                body.ApplyImpulseAt( LastDamage.Position, LastDamage.Force * 25.0f );
            }
        }*/

        OnBreak.Fire(LastDamage.Attacker);
    }

    public async Task ExplodeAsync(float fTime)
    {
        if (LifeState != LifeState.Alive && LifeState != LifeState.Dying)
            return;

        LifeState = LifeState.Dead;

        await Task.DelaySeconds(fTime);

        DoGibs();
        DoExplosion();

        Delete();
    }

    private void DoExplosion()
    {
        if (Model == null || Model.IsError)
            return;

        if (!Model.TryGetData(out ModelExplosionBehavior explosionBehavior))
            return;

        var srcPos = Position;
        if (PhysicsBody.IsValid()) srcPos = PhysicsBody.MassCenter;
        // Damage and push away all other entities
        if (explosionBehavior.Radius > 0.0f)
        {
            new ExplosionEntity
            {
                Position = srcPos,
                Radius = explosionBehavior.Radius,
                Damage = explosionBehavior.Damage,
                ForceScale = explosionBehavior.Force,
                ParticleOverride = explosionBehavior.Effect,
                SoundOverride = explosionBehavior.Sound
            }.Explode(this);
        }
    }

}