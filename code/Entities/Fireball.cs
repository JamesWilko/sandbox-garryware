using System;
using Sandbox;

namespace Garryware.Entities;

public class Fireball : BasePhysics
{
    public event Action<GarrywarePlayer> TouchedPlayer;
    
    private Particles fireParticles;
    private GarrywarePlayer currentTarget;
    
    private float moveSpeed;
    private Vector3 knockbackVelocity;
    private float accumulatedDamage;

    private const float Size = 20f;
    private const float DamageToSwitchTarget = 9 * 4 - 1;
    private static readonly ShuffledDeck<float> SpeedsDeck = new();

    static Fireball()
    {
        SpeedsDeck.Clear();
        SpeedsDeck.Add(220f);
        SpeedsDeck.Add(240f);
        SpeedsDeck.Add(260f);
        SpeedsDeck.Add(280f);
        SpeedsDeck.Add(300f);
        SpeedsDeck.Shuffle();
    }
    
    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromSphere(PhysicsMotionType.Keyframed, Vector3.Zero, Size);
        EnableTraceAndQueries = true;
        EnableSolidCollisions = false;

        fireParticles = Particles.Create("particles/microgame.fireball.vpcf", this);
        Sound.FromEntity("fireball.spawn", this);
        Sound.FromEntity("fireball.ambient", this);

        SpeedsDeck.Shuffle();
        moveSpeed = SpeedsDeck.Next();
        PickNewTarget();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        fireParticles?.Destroy();
    }

    private void PickNewTarget()
    {
        currentTarget = TargetingUtility.GetRandomPlayerStillInPlay();
    }
    
    [GameEvent.Tick.Server]
    private void Tick()
    {
        if(currentTarget == null)
            return;
        
        // Chase target
        var directionToTarget = currentTarget.GetBoneTransform("spine_1").Position - Position;
        var turnSpeed = 0.01f;
        var idealRotation = Rotation.LookAt(directionToTarget, Vector3.Up);
        Rotation = Rotation.Slerp(Rotation, idealRotation, Time.Delta * turnSpeed);
        Rotation = Rotation.Clamp(idealRotation, 0.02f, out _);
        var nextLocation = Position + Rotation.Forward * moveSpeed * Time.Delta;
        
        // Check if we came into contact with a player
        var results = Trace.Sphere(Size * 1.1f, Position, nextLocation)
            .DynamicOnly()
            .WithTag("player")
            .RunAll();

        if (results != null)
        {
            foreach (var tr in results)
            {
                if (tr.Entity is GarrywarePlayer player)
                {
                    TouchedPlayer?.Invoke(player);
                    
                    // Pick a new target if we got who we were chasing
                    if (player == currentTarget)
                    {
                        PickNewTarget();
                    }
                }
            }
        }

        // Move into the new location
        Position = nextLocation;

        // Get knocked back over time if we've been damaged
        var velocityBleed = knockbackVelocity * 5f * Time.Delta;
        Position += velocityBleed;
        knockbackVelocity -= velocityBleed;
    }

    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);

        knockbackVelocity += info.Force;
        
        // If our target shoots this enough then start seeking someone else
        if (info.Attacker == currentTarget)
        {
            accumulatedDamage += info.Damage;
            if (accumulatedDamage >= DamageToSwitchTarget)
            {
                PickNewTarget();
                Particles.Create("particles/microgame.fireball.burst.vpcf", this).Destroy();
                Sound.FromEntity("fireball.hurt", this);
                accumulatedDamage = 0;
            }
        }
    }

    public void DisableCollisions()
    {
        EnableTraceAndQueries = false;
        EnableSolidCollisions = false;
    }
    
}
