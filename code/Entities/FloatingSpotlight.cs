using System;
using Sandbox;

namespace Garryware.Entities;

public partial class FloatingSpotlight : SpotLightEntity
{
    public float MaxSpeed { get; set; } = 1000.0f;
    private Rotation facingDownRotation;
    
    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/torch/torch.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
        
        facingDownRotation = new Angles(90f, 0f, 0f).ToRotation();
        
        // Set up physics properties so it floats around the arena
        PhysicsBody.LinearDrag = 0.5f;
        PhysicsBody.DragEnabled = true;
        PhysicsBody.GravityEnabled = false;

        Enabled = true;
        DynamicShadows = false;
        Range = 512f;
        Brightness = 10f;
        Color = Color.Yellow;
    }

    [Event.Physics.PostStep]
    protected void KeepRotationAndHeight()
    {
        if (!this.IsValid() || IsClient)
            return;

        var body = PhysicsBody;
        if (!body.IsValid())
            return;

        Position = body.Position.WithZ(128f);
        Rotation = facingDownRotation;
    }
    
    protected override void OnPhysicsCollision(CollisionEventData eventData)
    {
        var speed = eventData.This.PreVelocity.Length;
        var direction = Vector3.Reflect(eventData.This.PreVelocity.Normal, eventData.Normal.Normal).Normal;
        Velocity = direction * MathF.Min(speed, MaxSpeed);
    }

}
