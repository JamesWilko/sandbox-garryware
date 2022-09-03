namespace Garryware;

public class RocketProjectile : Projectile
{
    protected override void OnDetonate()
    {
        base.OnDetonate();
        
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
