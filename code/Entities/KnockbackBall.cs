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

        PhysicsBody.Mass = 20.0f;
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
}