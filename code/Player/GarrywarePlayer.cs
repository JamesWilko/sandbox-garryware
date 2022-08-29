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
    public bool HasWonRound => RoundResult == RoundResult.Won;
    public bool HasLostRound => RoundResult == RoundResult.Lost;
    
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

        Controller = new WalkController();
        DevController = null;

        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        Clothing.DressEntity(this);

        CameraMode = new FirstPersonCamera();

        base.Respawn();
    }

    public override void OnKilled()
    {
        base.OnKilled();

        if (lastDamage.Flags.HasFlag(DamageFlags.Vehicle))
        {
            Particles.Create("particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position);
            Particles.Create("particles/impact.flesh-big.vpcf", lastDamage.Position);
            PlaySound("kersplat");
        }

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
        TookDamage(lastDamage.Flags, lastDamage.Position, lastDamage.Force);
    }

    [ClientRpc]
    public void TookDamage(DamageFlags damageFlags, Vector3 forcePos, Vector3 force)
    {
    }

    public override PawnController GetActiveController()
    {
        if (DevController != null) return DevController;

        return base.GetActiveController();
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

        var controller = GetActiveController();
        if (controller != null)
        {
            EnableSolidCollisions = !controller.HasTag("noclip");

            SimulateAnimation(controller);
        }

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
                dropped.PhysicsGroup.ApplyImpulse(Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true);
                dropped.PhysicsGroup.ApplyAngularImpulse(Vector3.Random * 100.0f, true);

                timeSinceDropped = 0;
            }
        }
    }

    Entity lastWeapon;

    void SimulateAnimation(PawnController controller)
    {
        if (controller == null)
            return;

        // where should we be rotated to
        var turnSpeed = 0.02f;
        var idealRotation = Rotation.LookAt(Input.Rotation.Forward.WithZ(0), Vector3.Up);
        Rotation = Rotation.Slerp(Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed);
        Rotation = Rotation.Clamp(idealRotation, 45.0f, out var shuffle); // lock facing to within 45 degrees of look direction

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper(this);

        animHelper.WithWishVelocity(controller.WishVelocity);
        animHelper.WithVelocity(controller.Velocity);
        animHelper.WithLookAt(EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f);
        animHelper.AimAngle = Input.Rotation;
        animHelper.FootShuffle = shuffle;
        animHelper.DuckLevel = MathX.Lerp(animHelper.DuckLevel, controller.HasTag("ducked") ? 1 : 0, Time.Delta * 10.0f);
        animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
        animHelper.IsGrounded = GroundEntity != null;
        animHelper.IsSitting = controller.HasTag("sitting");
        animHelper.IsNoclipping = controller.HasTag("noclip");
        animHelper.IsClimbing = controller.HasTag("climbing");
        animHelper.IsSwimming = WaterLevel >= 0.5f;
        animHelper.IsWeaponLowered = false;

        if (controller.HasEvent("jump")) animHelper.TriggerJump();
        if (ActiveChild != lastWeapon) animHelper.TriggerDeploy();

        if (ActiveChild is BaseCarriable carry)
        {
            carry.SimulateAnimator(animHelper);
        }
        else
        {
            animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
            animHelper.AimBodyWeight = 0.5f;
        }

        lastWeapon = ActiveChild;
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
        // @todo: turn this into an event?
        RoundResult = RoundResult.Undecided;
    }
    
    public void FlagAsRoundWinner()
    {
        RoundResult = RoundResult.Won;
    }

    public void FlagAsRoundLoser()
    {
        RoundResult = RoundResult.Lost;
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
        foreach (var trigger in CommonEntities.OnBoxTriggers)
        {
            if (trigger.Contains(this))
            {
                return true;
            }
        }
        return false;
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
            var walkController = Controller as WalkController;
            return walkController.Velocity.Length > (walkController.SprintSpeed * 0.9f);
        }
    }
    
}