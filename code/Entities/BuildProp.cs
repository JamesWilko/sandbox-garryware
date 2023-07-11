using Sandbox;

namespace Garryware.Entities;

public class BuildProp : BreakableProp
{
    private bool IsGrabbed { get; set; }
    
    public override void OnGravityGunPickedUp(GravityGunInfo info)
    {
        base.OnGravityGunPickedUp(info);
        IsGrabbed = true;
    }

    public override void OnGravityGunDropped(GravityGunInfo info)
    {
        base.OnGravityGunDropped(info);
        
        IsGrabbed = false;
    }

    public override void OnGravityGunPunted(GravityGunInfo info)
    {
        base.OnGravityGunPunted(info);
        
        RenderColor = Color.Blue;
        PhysicsEnabled = false;
        IsGrabbed = false;
    }

    [GameEvent.Physics.PostStep]
    protected void StayLevel()
    {
        if (Game.IsServer && IsGrabbed)
        {
            var desiredRotation = Rotation.Angles().WithPitch(0.0f).WithRoll(0.0f).ToRotation();
            Rotation = Rotation.Lerp(Rotation, desiredRotation, Time.Delta * 20f);
            PhysicsBody.Velocity = Vector3.Zero;
            PhysicsBody.AngularVelocity = Vector3.Zero;
        }
    }
    
}
