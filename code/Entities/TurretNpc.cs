using System;
using System.Linq;
using Sandbox;

namespace Garryware.Entities;

public partial class TurretNpc : AnimatedEntity
{
    private ClothingContainer Clothing { get; set; } = new();
    private Inventory Inventory { get; set; }
    private NpcWeapon ActiveWeapon { get; set; }

    /// <summary>
    /// Position a player should be looking from in world space.
    /// </summary>
    public Vector3 EyePosition
    {
        get => Transform.PointToWorld(EyeLocalPosition);
        set => EyeLocalPosition = Transform.PointToLocal(value);
    }

    /// <summary>
    /// Position a player should be looking from in local to the entity coordinates.
    /// </summary>
    [Net, Predicted]
    public Vector3 EyeLocalPosition { get; set; }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
    /// </summary>
    public Rotation EyeRotation
    {
        get => Transform.RotationToWorld(EyeLocalRotation);
        set => EyeLocalRotation = Transform.RotationToLocal(value);
    }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
    /// </summary>
    [Net, Predicted]
    public Rotation EyeLocalRotation { get; set; }
    
    public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );
    
    public bool CanFire { get; set; }
    public TimeUntil TeagbagTime { get; set; }
    private TimeUntil TimeUntilPickANewTarget { get; set; }

    private GarrywarePlayer currentTarget;

    private bool isCrouching;
    private TimeUntil timeUntilInvertCrouch;

    // How long does this npc fire at a single player
    private const float AttentionSpan = 5f;
    private const float TeabagSpeed = 0.16f;

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/citizen/citizen.vmdl");
        
        SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius(80, 12));
        EnableAllCollisions = true;
        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        Inventory = new Inventory(this);
        GiveWeapon<NpcSmg>();

        // Load army clothing
        LoadClothing("models/citizen_clothes/", "skin01", "skin02", "skin03", "skin04", "skin05");
        LoadClothing("models/citizen_clothes/trousers/cargopants/", "cargo_pants_army");
        LoadClothing("models/citizen_clothes/shoes/boots/", "army_boots");
        LoadClothing("models/citizen_clothes/gloves/tactical_gloves/", "army_gloves", "tactical_gloves");
        LoadClothing("models/citizen_clothes/hat/tactical_helmet/", "tactical_helmet_army", "tactical_helmet");
        LoadClothing("models/citizen_clothes/shirt/army_shirt/", "army_shirt");
        Clothing.DressEntity(this);
    }

    /// <summary>
    /// Load a random one of the clothing items passed in 
    /// </summary>
    private void LoadClothing(string parentPath, params string[] items)
    {
        var item = Game.Random.FromArray(items);
        var path = $"{parentPath}{item}.clothing";
        if (ResourceLibrary.TryGet<Clothing>(path, out var clothingAsset))
        {
            Clothing.Clothing.Add(clothingAsset);
        }
    }
    
    protected void GiveWeapon<T>() where T : NpcWeapon, new()
    {
        Inventory.DeleteContents();

        ActiveWeapon = new T();
        Inventory.Add(ActiveWeapon, true);
        ActiveWeapon.ActiveStart(this);
    }

    protected void PickNewTarget()
    {
        var newTarget = TargetingUtility.GetRandomPlayerStillInPlay();
        if (currentTarget != newTarget)
        {
            ActiveWeapon?.OnChoseNewTarget();
        }

        currentTarget = newTarget;
        TimeUntilPickANewTarget = AttentionSpan;
    }

    public void OnEliminatedPlayer()
    {
        TeagbagTime = 1.5f;
        TimeUntilPickANewTarget = Math.Max(TeagbagTime, TimeUntilPickANewTarget);
    }

    [GameEvent.Tick.Server]
    protected virtual void Tick()
    {
        if (currentTarget == null || TimeUntilPickANewTarget <= 0)
        {
            PickNewTarget();
        }

        if (currentTarget == null)
            return;

        // Shoot at the chest
        var lookTarget = currentTarget.GetBoneTransform("spine_1").Position;
        var directionToTarget = (lookTarget - EyePosition).Normal;

        var turnSpeed = 0.02f;
        var idealRotation = Rotation.LookAt(directionToTarget.WithZ(0), Vector3.Up);
        Rotation = Rotation.Slerp(Rotation, idealRotation, Time.Delta * turnSpeed);
        Rotation = Rotation.Clamp(idealRotation, 45.0f, out var shuffle);
        
        EyePosition = Position + Vector3.Up * 60f;
        EyeRotation = directionToTarget.EulerAngles.ToRotation();

        var anim = new CitizenAnimationHelper(this);
        anim.WithLookAt(lookTarget);
        anim.WithVelocity(Velocity);
        anim.FootShuffle = shuffle;

        if (ActiveWeapon != null && ActiveWeapon.IsValid)
        {
            ActiveWeapon.SimulateAnimator(anim);

            if (CanFire && ActiveWeapon.CanPrimaryAttack())
                ActiveWeapon.AttackPrimary();
        }

        if (TeagbagTime > 0f && timeUntilInvertCrouch <= 0f)
        {
            isCrouching = !isCrouching;
            anim.DuckLevel = isCrouching ? 1f : 0f;
            timeUntilInvertCrouch = TeabagSpeed;
        }
    }
}