using System;
 using Garryware.Entities;
using Garryware.UI;
using Sandbox;

namespace Garryware;

public enum RoundResult
{
    Undecided,
    Lost,
    Won
}

public partial class GarrywarePlayer : Player
{
    private TimeSince timeSinceDropped;

    private DamageInfo lastDamage;

    [Net, Change(nameof(OnRoundStateChanged))]
    protected RoundResult RoundResult { get; set; }

    public bool HasLockedInResult => RoundResult != RoundResult.Undecided;
    public int LockedInResultOnTick { get; private set; }
    public bool HasWonRound => RoundResult == RoundResult.Won;
    public bool HasLostRound => RoundResult == RoundResult.Lost;

    [Net] public bool WasHereForRoundStart { get; set; }

    public bool IsDucking => Controller.HasTag("ducked");
    private bool WasDucking { get; set; }
    private TimeSince TimeSinceSwitchingStance { get; set; }
    public bool IsSquatting { get; set; }
    public event Action<GarrywarePlayer> Squatted; 

    private const float SquatDownTime = 0.15f;
    private const float SquatUpTime = 0.1f;
    
    public delegate void DamageDelegate(GarrywarePlayer victim, DamageInfo info);
    public event DamageDelegate Hurt;
    
    /// <summary>
    /// The clothing container is what dresses the citizen
    /// </summary>
    public ClothingContainer Clothing = new();

    /// <summary>
    /// Default init
    /// </summary>
    public GarrywarePlayer()
    {
        Inventory = new Inventory(this);
    }

    /// <summary>
    /// Initialize using this client
    /// </summary>
    public GarrywarePlayer(Client cl) : this()
    {
        // Load clothing from client data
        Clothing.LoadFromClient(cl);
    }

    public override void Respawn()
    {
        SetModel("models/citizen/citizen.vmdl");

        Controller = new GarrywareWalkController();

        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        Clothing.DressEntity(this);

        Animator = new GarrywarePlayerAnimator();

        CameraMode = new FirstPersonCamera();

        base.Respawn();
    }

    public override void OnKilled()
    {
        base.OnKilled();

        BecomeRagdollOnClient(Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone(lastDamage.HitboxIndex));

        Controller = null;

        EnableAllCollisions = false;
        EnableDrawing = false;

        CameraMode = new SpectateRagdollCamera();

        foreach (var child in Children)
        {
            child.EnableDrawing = false;
        }

        Inventory.DropActive();
        Inventory.DeleteContents();
    }
    
    public override void TakeDamage(DamageInfo info)
    {
        // Functionally give the players god mode in the game since we don't want anybody to be able to die
        info.Damage = 0.0f;
        
        base.TakeDamage(info);
        
        lastDamage = info;
        Hurt?.Invoke(this, info);
        
        // Apply knockback from explosions
        if (info.Flags.HasFlag(DamageFlags.Blast) && Controller is GarrywareWalkController controller)
        {
            var knockbackForce = lastDamage.Force;
            const float rocketJumpVerticalForceMultiplier = 2.2f;
            
            // Rocket-jump detection
            if (GroundEntity == null)
            {
                var directionToExplosion = (info.Position - Position).Normal;
                if (Vector3.Dot(directionToExplosion, Vector3.Down) > 0.7f)
                {
                    knockbackForce.z *= rocketJumpVerticalForceMultiplier;
                }
            }

            controller.Knockback(knockbackForce);
        }
    }

    public void Knockback(Vector3 force)
    {
        if(Controller is GarrywareWalkController controller)
            controller.Knockback(force);
    }
    
    public override void Simulate(Client cl)
    {
        base.Simulate(cl);

        if (Input.ActiveChild != null)
        {
            ActiveChild = Input.ActiveChild;
        }

        if (LifeState != LifeState.Alive)
            return;

        TickPlayerUse();
        SimulateActiveChild(cl, ActiveChild);

        /*
        // Swap camera between FPS and TPS
        if (Input.Pressed(InputButton.View))
        {
            if (CameraMode is ThirdPersonCamera)
            {
                CameraMode = new FirstPersonCamera();
            }
            else
            {
                CameraMode = new ThirdPersonCamera();
            }
        }
        */

        // Drop the currently held weapon
        if (Input.Pressed(InputButton.Drop))
        {
            var dropped = Inventory.DropActive();
            if (dropped != null)
            {
                if (dropped is AmmoWeapon weapon)
                {
                    weapon.LastOwner = this;
                }
                
                dropped.PhysicsGroup.ApplyImpulse(Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true);
                dropped.PhysicsGroup.ApplyAngularImpulse(Vector3.Random * 100.0f, true);
                dropped.PhysicsGroup.SetSurface("metal.weapon.dropped");

                timeSinceDropped = 0;
            }
        }
        
        // Squat detection
        if (IsDucking && !WasDucking)
        {
            TimeSinceSwitchingStance = 0.0f;
        }
        else if(!IsDucking && WasDucking)
        {
            TimeSinceSwitchingStance = 0.0f;
        }
        WasDucking = IsDucking;

        if (IsDucking && !IsSquatting && TimeSinceSwitchingStance > SquatDownTime)
        {
            IsSquatting = true;
        }
        if (!IsDucking && IsSquatting && TimeSinceSwitchingStance > SquatUpTime)
        {
            IsSquatting = false;
            Squatted?.Invoke(this);
        }
        
    }

    public override void StartTouch(Entity other)
    {
        if (timeSinceDropped < 1) return;

        base.StartTouch(other);
    }

    public override float FootstepVolume()
    {
        return Velocity.WithZ(0).Length.LerpInverse(0.0f, 200.0f) * 5.0f;
    }

    public bool IsUseDisabled()
    {
        return ActiveChild is IUse use && use.IsUsable(this);
    }

    protected override Entity FindUsable()
    {
        if (IsUseDisabled())
            return null;

        // First try a direct 0 width line
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * (85 * Scale))
            .WithoutTags("trigger")
            .Ignore(this)
            .Run();

        // See if any of the parent entities are usable if we ain't.
        var ent = tr.Entity;
        while (ent.IsValid() && !IsValidUseEntity(ent))
        {
            ent = ent.Parent;
        }

        // Nothing found, try a wider search
        if (!IsValidUseEntity(ent))
        {
            tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * (85 * Scale))
                .WithoutTags("trigger")
                .Radius(2)
                .Ignore(this)
                .Run();

            // See if any of the parent entities are usable if we ain't.
            ent = tr.Entity;
            while (ent.IsValid() && !IsValidUseEntity(ent))
            {
                ent = ent.Parent;
            }
        }

        // Still no good? Bail.
        if (!IsValidUseEntity(ent)) return null;

        return ent;
    }

    protected override void UseFail()
    {
        if (IsUseDisabled())
            return;

        base.UseFail();
    }

    [ClientRpc]
    private void BecomeRagdollOnClient(Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone)
    {
        var ent = new ModelEntity();
        ent.Tags.Add("ragdoll", "solid", "debris");
        ent.Position = Position;
        ent.Rotation = Rotation;
        ent.Scale = Scale;
        ent.UsePhysicsCollision = true;
        ent.EnableAllCollisions = true;
        ent.SetModel(GetModelName());
        ent.CopyBonesFrom(this);
        ent.CopyBodyGroups(this);
        ent.CopyMaterialGroup(this);
        ent.CopyMaterialOverrides(this);
        ent.TakeDecalsFrom(this);
        ent.EnableAllCollisions = true;
        ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
        ent.RenderColor = RenderColor;
        ent.PhysicsGroup.Velocity = velocity;
        ent.PhysicsEnabled = true;

        foreach (var child in Children)
        {
            if (!child.Tags.Has("clothes")) continue;
            if (child is not ModelEntity e) continue;

            var model = e.GetModelName();

            var clothing = new ModelEntity();
            clothing.SetModel(model);
            clothing.SetParent(ent, true);
            clothing.RenderColor = e.RenderColor;
            clothing.CopyBodyGroups(e);
            clothing.CopyMaterialGroup(e);
        }

        if (damageFlags.HasFlag(DamageFlags.Bullet) ||
            damageFlags.HasFlag(DamageFlags.PhysicsImpact))
        {
            PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody(bone) : null;

            if (body != null)
            {
                body.ApplyImpulseAt(forcePos, force * body.Mass);
            }
            else
            {
                ent.PhysicsGroup.ApplyImpulse(force);
            }
        }

        if (damageFlags.HasFlag(DamageFlags.Blast))
        {
            if (ent.PhysicsGroup != null)
            {
                ent.PhysicsGroup.AddVelocity((Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f));
                var angularDir = (Rotation.FromYaw(90) * force.WithZ(0).Normal).Normal;
                ent.PhysicsGroup.AddAngularVelocity(angularDir * (force.Length * 0.02f));
            }
        }

        Corpse = ent;

        ent.DeleteAsync(10.0f);
    }

    public void ResetRound()
    {
        RoundResult = RoundResult.Undecided;
        LockedInResultOnTick = -1;
        WasHereForRoundStart = true;
    }
    
    public void FlagAsRoundWinner()
    {
        if(!WasHereForRoundStart || HasLockedInResult)
            return;
        
        RoundResult = RoundResult.Won;
        LockedInResultOnTick = Time.Tick;
        GarrywareGame.Current.UpdateWinLoseCounts();
    }

    public void FlagAsRoundLoser()
    {
        if(!WasHereForRoundStart || HasLockedInResult)
            return;
        
        RoundResult = RoundResult.Lost;
        LockedInResultOnTick = Time.Tick;
        GarrywareGame.Current.UpdateWinLoseCounts();
    }
    
    public void OnRoundStateChanged(RoundResult oldResult, RoundResult newResult)
    {
        GameEvents.PlayerLockedInResult(Client, newResult);
    }

    /// <summary>
    /// Is the player standing on one of the boxes in the garryware map?
    /// </summary>
    public bool IsOnABox()
    {
        return Tags.Has(Garryware.Tags.OnBox);
    }

    /// <summary>
    /// Gets the OnBoxTrigger that the player is currently in, or null if not in one
    /// </summary>
    public Entities.OnBoxTrigger GetOnBoxTrigger()
    {
        if (!Tags.Has(Garryware.Tags.OnBox)) return null;
        foreach (var trigger in GarrywareGame.Current.CurrentRoom.OnBoxTriggers)
        {
            if (trigger.ContainsEntity(this))
            {
                return trigger;
            }
        }
        return null;
    }

    /// <summary>
    /// Is the player standing on the floor and not one of the boxes in the garryware map? 
    /// </summary>
    public bool IsOnTheFloor()
    {
        return Position.z < 1.0f && GroundEntity != null;
    }
    
    /// <summary>
    /// Remove all weapons from this player.
    /// </summary>
    public void RemoveWeapons()
    {
        Inventory.DeleteContents();
    }
    
    public bool IsMovingAtSprintSpeed
    {
        get
        {
            if (Controller is WalkController walkController)
            {
                return walkController.Velocity.Length > (walkController.SprintSpeed * 0.9f);
            }
            return false;
        }
    }

    public void TeleportTo(Transform transform)
    {
        Transform = transform;
        Sound.FromEntity("garryware.player.teleport", this);
        Particles.Create("particles/player/player_teleport.vpcf", this);
    }

    public bool IsInAChair
    {
        get
        {
            return Parent is Chair;
        }
    }

    public void OverrideCameraType(Type cameraType)
    {
        CameraMode = TypeLibrary.Create<CameraMode>(cameraType);
    }

    public void RestoreNormalCamera()
    {
        CameraMode = new FirstPersonCamera();
    }
    
}
