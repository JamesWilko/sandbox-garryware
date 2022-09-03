using System.Collections.Generic;
using Sandbox;

namespace Garryware;

public class ProjectileWeapon<T> : AmmoWeapon where T : Entity, new()
{
    public virtual float ProjectileLaunchSpeed => 500.0f;
    public virtual float LaunchDistanceOffset => 20.0f;
    
    protected List<Entity> OwnedProjectiles { get; private set; } = new();
    
    public override bool CanPrimaryAttack()
    {
        return base.CanPrimaryAttack() && Input.Pressed(InputButton.PrimaryAttack);
    }
    
    public override void AttackPrimary()
    {
        (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
        ShootEffects();
        ShootProjectile(0f, ProjectileLaunchSpeed);
        TakeAmmo(1);
    }
    
    public override void Simulate(Client owner)
    {
        base.Simulate(owner);

        for (int i = OwnedProjectiles.Count - 1; i >= 0; --i)
        {
            var projectile = OwnedProjectiles[i];
            if (!projectile.IsValid)
            {
                OwnedProjectiles.RemoveAt(i);
                continue;
            }
            
            projectile.Simulate(owner);
        }
    }
    
    protected virtual void ShootProjectile(float spread, float launchSpeed)
    {
        if(IsClient)
            return;
        
        Rand.SetSeed(Time.Tick);
        var forward = Owner.EyeRotation.Forward;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;

        using (Prediction.Off())
        {
            var location = Owner.EyePosition + Owner.EyeRotation.Forward * LaunchDistanceOffset;
            var rotation = Owner.EyeRotation;
            var velocity = forward * launchSpeed;
            OwnedProjectiles.Add(CreateProjectile(location, rotation, velocity));
        }
    }

    protected virtual Entity CreateProjectile(Vector3 location, Rotation rotation, Vector3 velocity)
    {
        return new T
        {
            Owner = Owner,
            Position = location,
            Rotation = rotation,
            Velocity = velocity,
        };
    }
    
}
