using Sandbox;
using System;
using System.Threading.Tasks;

namespace Garryware.Entities;

/// <summary>
/// A prop that physically simulates as a single rigid body. It can be constrained to other physics objects using hinges
/// or other constraints. It can also be configured to break when it takes enough damage.
/// Note that the health of the object will be overridden by the health inside the model, to ensure consistent health game-wide.
/// If the model used by the prop is configured to be used as a prop_dynamic (i.e. it should not be physically simulated) then it CANNOT be
/// used as a prop_physics. Upon level load it will display a warning in the console and remove itself. Use a prop_dynamic instead.
/// </summary>
public partial class BreakableProp : BasePhysics, IGravityGunCallback
{
    /// <summary>
    /// If set, the prop will spawn with motion disabled and will act as a nav blocker until broken.
    /// </summary>
    [Property, Net]
    public bool Static { get; set; } = false;

    /// <summary>
    /// If set, the prop will spawn its associated gibs when it is destroyed.
    /// </summary>
    [Property]
    public bool CanGib { get; set; } = true;
    
    /// <summary>
    /// If set, the gibs from the prop will burst from their location when spawned.
    /// </summary>
    [Property]
    public bool CanGibsBurst { get; set; } = true;
    
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

    /// <summary>
    /// Makes this entity indestructible but it still raises damage events.
    /// </summary>
    [Net] public bool Indestructible { get; set; } = false;
    
    /// <summary>
    /// Backing property for the GameColor property which also sets the render color automatically.
    /// </summary>
    [Net, Change(nameof(OnInternalGameColorChanged))] private GameColor InternalColor { get; set; } = GameColor.White;
    
    /// <summary>
    /// Set the color of the prop used for gameplay purposes.
    /// </summary>
    public GameColor GameColor
    {
        get => InternalColor;
        set
        {
            InternalColor = value;
            RenderColor = value.AsColor();
        }
    }

    /// <summary>
    /// Backing property for the HideGameColor property.
    /// </summary>
    [Net, Change(nameof(OnHideGameColorChanged))] public bool InternalHideGameColor { get; set; }

    /// <summary>
    /// Should game color be hidden. The game color will still be active, but players won't see the prop as that color.
    /// </summary>
    public bool HideGameColor
    {
        get => InternalHideGameColor;
        set => InternalHideGameColor = value;
    }
    
    /// <summary>
    /// Should this prop raise our client authoritative damage event when it is damaged.
    /// This is so that high latency players can still get accurate results when a microgame constantly and quickly changes the properties of this prop. 
    /// </summary>
    [Net] public bool RaisesClientAuthDamageEvent { get; set; }
    
    /// <summary>
    /// Should a 3d text panel be shown in the world over this prop?
    /// </summary>
    [Net, Change(nameof(OnShowWorldTextChanged))] public bool ShowWorldText { get; set; }
    
    /// <summary>
    /// What text should be shown on the 3d text panel?
    /// </summary>
    [Net] public string WorldText { get; set; }

    private EntityWorldTextPanel WorldTextPanel { get; set; }
        
    public delegate void AttackerDelegate(BreakableProp self, Entity attacker);
    public event AttackerDelegate Damaged;
    public event AttackerDelegate OnBroken;

    public delegate void CollisionEventDelegate(CollisionEventData collisionData);
    public event CollisionEventDelegate PhysicsCollision;
    
    public override void Spawn()
    {
        base.Spawn();
        
        UsePhysicsCollision = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;
        EnableLagCompensation = true;
        Tags.Add("prop", "solid");

        if (Static)
        {
            SetupPhysicsFromModel(PhysicsMotionType.Static);
        }
        else
        {
            PhysicsEnabled = true;
            SetupPhysics();
        }

        UpdateGameColorMaterialOverride();
        
        // @todo: check if a microgame is running and automatically add this ent to the auto-cleanup
    }
    
    public override void ClientSpawn()
    {
        base.ClientSpawn();

        if (ShowWorldText)
        {
            CreateWorldText();
        }
    }
    
    protected virtual void SetupPhysics()
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
        UpdateGameColorMaterialOverride();
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
    
    protected override void OnDestroy()
    {
        base.OnDestroy();

        AttemptDeleteWorldText();
    }

    protected DamageInfo LastDamage;

    /// <summary>
    /// Fired when the entity gets damaged.
    /// </summary>
    protected Output OnDamaged { get; set; }

    public override void TakeDamage(DamageInfo info)
    {
        LastDamage = info;

        ApplyDamageForces(info);
        
        LastAttacker = info.Attacker;
        LastAttackerWeapon = info.Weapon;

        if (IsClient && RaisesClientAuthDamageEvent)
        {
            TakeClientDamage(info);
        }
        
        if (!IsServer || LifeState != LifeState.Alive)
            return;

        if (!Indestructible)
        {
            Health -= info.Damage;
            if (Health <= 0.0 && LifeState == LifeState.Alive)
            {
                Health = 0.0f;
                OnKilled();
            }
        }

        // TODO: Add damage type as argument? Or should it be the new health value?
        OnDamaged.Fire(this);
        Damaged?.Invoke(this, LastDamage.Attacker);
    }

    protected virtual void TakeClientDamage(DamageInfo info)
    {
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
        OnBroken?.Invoke(this, LastDamage.Attacker);
        
        base.OnKilled();
    }

    CollisionEventData lastCollision;

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        lastCollision = eventData;

        base.OnPhysicsCollision(eventData);
        
        PhysicsCollision?.Invoke(eventData);
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
        Debris.Add(result.Props);

        if (CanGibsBurst)
        {
            foreach (var prop in result.Props)
            {
                if (!prop.IsValid())
                    continue;

                var body = prop.PhysicsBody;
                if (!body.IsValid())
                    continue;

                body.ApplyImpulseAt(WorldSpaceBounds.Center, (prop.Position - WorldSpaceBounds.Center) * Rand.Float(50f, 250f));
            }
        }

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
    
    private void OnInternalGameColorChanged(GameColor oldColor, GameColor newColor)
    {
        UpdateGameColorMaterialOverride();
    }

    private void UpdateGameColorMaterialOverride()
    {
        if (IsServer) return;
        
        if (!HideGameColor && GameColor != GameColor.White)
        {
            SetMaterialOverride(CommonEntities.WhiteMaterial);
        }
        else
        {
            ClearMaterialOverride();
        }
    }
    
    private void OnHideGameColorChanged(bool oldValue, bool newValue)
    {
        RenderColor = newValue ? Color.White : GameColor.AsColor();
        UpdateGameColorMaterialOverride();
    }
    
    [Net] public Client ClientLastPickedUpBy { get; set; }

    public virtual void OnGravityGunPickedUp(GravityGunInfo info)
    {
        ClientLastPickedUpBy = info.Instigator;
    }

    public virtual void OnGravityGunDropped(GravityGunInfo info)
    {
        ClientLastPickedUpBy = info.Instigator;
    }

    public virtual void OnGravityGunPunted(GravityGunInfo info)
    {
        ClientLastPickedUpBy = info.Instigator;
    }
    
    private void CreateWorldText()
    {
        AttemptDeleteWorldText();
        
        WorldTextPanel = new EntityWorldTextPanel
        {
            Transform = Transform,
            Owner = this
        };
    }
    
    private void AttemptDeleteWorldText()
    {
        if (!IsClient || WorldTextPanel == null)
            return;
        
        WorldTextPanel.Delete();
        WorldTextPanel = null;
    }

    private void OnShowWorldTextChanged(bool oldValue, bool newValue)
    {
        if (ShowWorldText)
        {
            CreateWorldText();
        }
        else
        {
            AttemptDeleteWorldText();
        }
    }

    [Event.Tick.Client]
    private void UpdateWorldTextPanelTick()
    {
        if (ShowWorldText && WorldTextPanel == null)
        {
            CreateWorldText();
        }
        else if(!ShowWorldText)
        {
            AttemptDeleteWorldText();
        }
        
        if (WorldTextPanel != null)
        {
            WorldTextPanel.Text = WorldText;
        }
    }

    
}
