using Sandbox;

namespace Garryware;

public partial class Projectile : ModelEntity
{
    public virtual Model ProjectileModel => CommonEntities.Ball;
    public virtual Rotation RotationOffset => Rotation.Identity;
    
    /// <summary>
    /// How long does this projectile live for?
    /// </summary>
    public float Lifetime { get; set; } = 3.0f;

    /// <summary>
    /// How big is the collision radius on this projectile?
    /// </summary>
    public float Radius { get; set; } = 5.0f;

    /// <summary>
    /// Should the entity be rotated to face the direction it is travelling in
    /// </summary>
    public bool FaceDirectionOfVelocity { get; set; } = true;
    
    [Predicted] protected TimeSince TimeSinceSpawned { get; set; }
    
    [Predicted] protected bool HasDetonated { get; set; }

    public override void Spawn()
    {
        base.Spawn();

        Model = ProjectileModel;
        PhysicsEnabled = false;
        UsePhysicsCollision = false;
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
        
        TimeSinceSpawned = 0;
    }

    public override void Simulate(Client cl)
    {
        base.Simulate(cl);

        var endLocation = Position + Velocity * Time.Delta;
        var tr = Trace.Sphere(Radius, Position, endLocation)
            .Ignore(this)
            .Ignore(Owner)
            .UseHitboxes()
            .WithAnyTags("solid", "player", "npc", "glass")
            .Run();

        if (FaceDirectionOfVelocity)
        {
            Rotation = Rotation.LookAt(Velocity.Normal) * RotationOffset;
        }

        if (tr.Hit && tr.Entity.IsValid)
        {
            Position += Velocity.Normal * tr.Distance;
            Detonate();
        }
        else
        {
            Position = endLocation;
            if (TimeSinceSpawned > Lifetime)
            {
                Detonate();
            }
        }
    }

    protected virtual void Detonate()
    {
        if(HasDetonated)
            return;

        using (Prediction.Off())
        {
            OnDetonate();
        }
        HasDetonated = true;
        Delete();
    }

    protected virtual void OnDetonate()
    {
    }

}
