using Sandbox;
using System;

namespace Garryware.Entities;

public partial class KnockbackBall : BreakableProp
{
    private TimeSince timeSinceSpawn;
    
    public override void Spawn()
    {
        Model = CommonEntities.BeachBall;
        Indestructible = true;
        base.Spawn();
        
        timeSinceSpawn = 0;
    }
    
    [Event.Tick.Server]
    private void LifetimeTick()
    {
        if(timeSinceSpawn > 3f)
            Delete();
    }

    protected override void OnDestroy()
    {
        if (IsClient)
        {
            Particles.Create("particles/impact.smokepuff.vpcf", Position).Destroy();
            Sound.FromWorld("sounds/balloon_pop_cute.sound", Position);
        }
        base.OnDestroy();
    }

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        base.OnPhysicsCollision(eventData);
        
        // Don't knockback with the person who fired this for just a short time 
        if(eventData.Other.Entity == Owner && timeSinceSpawn < 0.1f)
            return;
        
        const float minSpeedToKnockback = 300f;
        
        // If we hit a player hard enough, then knock them around
        if (eventData.Speed >= minSpeedToKnockback
            && eventData.Other.Entity is GarrywarePlayer player
            && player.Controller is GarrywareWalkController controller)
        {
            var speed = (eventData.Speed - minSpeedToKnockback) * 0.5f;
            var knockbackForce = eventData.Velocity.Normal * speed;
            controller.Knockback(knockbackForce);
        }
        
    }
    
}
