using System;
using System.Linq;
using Sandbox;

namespace Garryware.Entities;

public class TurretNpc : AnimatedEntity
{
    private ClothingContainer Clothing { get; set; } = new();
    private Inventory Inventory { get; set; }
    private NpcWeapon ActiveWeapon { get; set; }
    
    public bool CanFire { get; set; }
    public TimeUntil TeagbagTime { get; set; }
    private TimeUntil TimeUntilPickANewTarget { get; set; }
    
    private GarrywarePlayer currentTarget;
    
    private bool isCrouching;
    private TimeUntil timeUntilInvertCrouch;

    // How long does this npc fire at a single player
    private const float AttentionSpan = 6f;
    private const float TeabagSpeed = 0.2f;
    
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
        // @todo: less naked
        Clothing.DressEntity(this);
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
        var newTarget = Rand.FromList(Client.All.ToList()).Pawn as GarrywarePlayer;
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
    
    [Event.Tick.Server]
    protected virtual void Tick()
    {
        if (currentTarget == null || TimeUntilPickANewTarget <= 0)
        {
            PickNewTarget();
        }
        
        if(currentTarget == null)
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
            
            if(CanFire && ActiveWeapon.CanPrimaryAttack())
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
