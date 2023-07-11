using System;
using Sandbox;

namespace Garryware.Entities;

public class KnockbackPlatform : BiggerPlatform
{

    [GameEvent.Tick.Server]
    protected void TickBouncePlayersAboveThisPlatform()
    {
        var bounds = CollisionBounds * Scale;
        var results = Trace.Box(bounds, Position, Position + Vector3.Up * 50f)
            .DynamicOnly()
            .WithTag("player")
            .RunAll();
        
        if(results == null)
            return;

        foreach (var result in results)
        {
            if (result.Entity is GarrywarePlayer player && player.Velocity.z <= 0f)
            {
                const float maxAngle = 30f;
        
                var x = (float)Math.Sin(Game.Random.Float(-maxAngle, maxAngle).DegreeToRadian());
                var y = (float)Math.Sin(Game.Random.Float(-maxAngle, maxAngle).DegreeToRadian());
                var direction = new Vector3(x, y, 1f);
                player.Knockback(direction * Game.Random.Float(650f, 800f));
                
                Particles.Create("particles/microgame.confetti.burst.vpcf", player).Destroy();
                Sound.FromEntity("garryware.sfx.confetti.pop", player);
            }
        }
        
    }
    
}
