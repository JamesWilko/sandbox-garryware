using Sandbox;
using System;

namespace Garryware.Entities;

public partial class BouncyBall : Prop
{
    public float MaxSpeed { get; set; } = 1000.0f;

    public delegate void EntityHitDelegate(BouncyBall self, CollisionEventData eventData);

    public event EntityHitDelegate EntityHit;

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/ball/ball.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic, false);
        Scale = Rand.Float(1.5f, 2.0f);
        RenderColor = Color.Random;
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
}
