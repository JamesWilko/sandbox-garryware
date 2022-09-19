using Sandbox;

namespace Garryware;

public class RocketProjectile : Projectile
{
    public override Model ProjectileModel => Model.Load("models/weapons/weapon.rpg.rocket.vmdl");
    public override Rotation RotationOffset => new Angles(90f, 0, 0).ToRotation();

    private Particles trail;
    
    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        trail = Particles.Create("particles/weapon.rpg.rocket.trail.vpcf");
        trail.SetEntity(0, this, true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        trail?.Destroy();
    } 
    
    protected override void OnDetonate()
    {
        base.OnDetonate();
        
        trail?.Destroy();
        trail = null;
        
        var boom = new ExplosionEntity()
        {
            Position = Position,
            Rotation = Rotation,
            UseForcePositionOverride = true,
            ForcePosition = Position - Vector3.Up * 50.0f,
            ParticleOverride = "particles/explosion/barrel_explosion/explosion_barrel.vpcf",
            SoundOverride = "weapon.rpg.explosion",
            Radius = 200.0f,
            Damage = 0f,
            ForceScale = 1.0f,
            RemoveOnExplode = true
        };
        boom.Explode(this);
    }
}
