using Sandbox;
using SandboxEditor;
using System;

namespace Garryware;

// @note: originally copied from gamemode Base

/// <summary>
/// An entity that creates an explosion at its center.
/// </summary>
[Library("ent_explosion"), HammerEntity]
[EditorSprite("editor/env_explosion.vmat"), Sphere("radius")]
[Title("Explosion"), Category("Effects"), Icon("radar")]
public partial class ExplosionEntity : Entity
{
    [ConVar.Server]
    static bool debug_prop_explosion { get; set; } = false;

    /// <summary>
    /// Radius of the explosion.
    /// </summary>
    [Property]
    public float Radius { get; set; } = 100.0f;

    /// <summary>
    /// Damage the exploision should do at the center. The damage will reduce the farther the target is from the center of the explosion.
    /// </summary>
    [Property]
    public float Damage { get; set; } = 100.0f;

    /// <summary>
    /// Scale explosion induced physics forces by this amount.
    /// </summary>
    [Property]
    public float ForceScale { get; set; } = 1.0f;
    
    /// <summary>
    /// If set, will override the default explosion partile effect.
    /// </summary>
    [ResourceType("vpcf")]
    [Property]
    public string ParticleOverride { get; set; }

    /// <summary>
    /// If set, will override the default explosion sound.
    /// </summary>
    [FGDType("sound")]
    [Property]
    public string SoundOverride { get; set; }

    /// <summary>
    /// Delete this entity when it is triggered via the Explode input?
    /// </summary>
    [Property]
    public bool RemoveOnExplode { get; set; } = true;

    public bool UseForcePositionOverride { get; set; } = false;
    
    public Vector3 ForcePosition { get; set; }
    
    // TODO: Tag list of entities to ignore/not damage?
    // TODO: Damage type override?
    // TODO: Empty Sound/Particle = no sound/particle? Or have separate "No sound/no particle" properties?

    /// <summary>
    /// Perform the explosion.
    /// </summary>
    /// <param name="activator">The entity to be responsible for the damage.</param>
    [Input]
    public void Explode(Entity activator)
    {
        // Effects
        Sound.FromWorld(string.IsNullOrWhiteSpace(SoundOverride) ? "rust_pumpshotgun.shootdouble" : SoundOverride, Position);
        Particles.Create(string.IsNullOrWhiteSpace(ParticleOverride) ? "particles/explosion/barrel_explosion/explosion_barrel.vpcf" : ParticleOverride, Position);

        // Damage, etc
        var overlaps = Entity.FindInSphere(Position, Radius);

        if (debug_prop_explosion)
            DebugOverlay.Sphere(Position, Radius, Color.Orange, 5, true);

        foreach (var overlap in overlaps)
        {
            if (overlap is not ModelEntity ent || !ent.IsValid())
                continue;

            if (ent.LifeState != LifeState.Alive)
                continue;

            if (!ent.PhysicsBody.IsValid())
                continue;

            if (ent.IsWorld)
                continue;

            var targetPos = ent.PhysicsBody.MassCenter;

            var damageDist = Vector3.DistanceBetween(Position, targetPos);
            if (damageDist > Radius)
                continue;

            var tr = Trace.Ray(Position, targetPos)
                .Ignore(activator)
                .WorldOnly()
                .Run();

            if (tr.Fraction < 0.95f)
            {
                if (debug_prop_explosion)
                    DebugOverlay.Line(Position, tr.EndPosition, Color.Red, 5, true);

                continue;
            }

            if (debug_prop_explosion)
                DebugOverlay.Line(Position, targetPos, 5, true);
            
            var damageDistanceMul = 1.0f - Math.Clamp(damageDist / Radius, 0.0f, 1.0f);
            var damage = Damage * damageDistanceMul;
            
            var forcePos = UseForcePositionOverride ? ForcePosition : Position;
            var forceDist  = Vector3.DistanceBetween(forcePos, targetPos);
            var forceDistanceMul = 1.0f - Math.Clamp(forceDist / Radius, 0.0f, 1.0f);
            var force = (ForceScale * forceDistanceMul) * ent.PhysicsBody.Mass;
            var forceDir = (targetPos - forcePos).Normal;

            ent.TakeDamage(DamageInfo.Explosion(Position, forceDir * force, damage)
                .WithAttacker(activator));
        }

        if (RemoveOnExplode)
            Delete();
    }
}