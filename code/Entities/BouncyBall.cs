using Sandbox;
using System;

namespace Garryware.Entities;

public partial class BouncyBall : BreakableProp
{
    public float MaxSpeed { get; set; } = 1000.0f;

    public delegate void EntityHitDelegate(BouncyBall self, CollisionEventData eventData);
    public event EntityHitDelegate EntityHit;
    
    public delegate void GravityGunEventDelegate(BouncyBall self, GravityGunInfo info);
    public event GravityGunEventDelegate Caught;

    public override void Spawn()
    {
        Model = CommonEntities.BeachBall;
        Scale = Rand.Float(1.5f, 2.0f);
        Indestructible = true;
        base.Spawn();
    }

    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        var speed = eventData.This.PreVelocity.Length;
        var direction = Vector3.Reflect(eventData.This.PreVelocity.Normal, eventData.Normal.Normal).Normal;
        Velocity = direction * MathF.Min(speed, MaxSpeed);

        if (eventData.Other.Entity != null)
        {
            EntityHit?.Invoke(this, eventData);
        }
    }

    public override void OnGravityGunPickedUp(GravityGunInfo info)
    {
        base.OnGravityGunPickedUp(info);
        Caught?.Invoke(this, info);
    }
}
