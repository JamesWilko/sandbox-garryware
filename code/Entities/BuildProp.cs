using Sandbox;

namespace Garryware.Entities;

public class BuildProp : BreakableProp
{
    public override void OnGravityGunPickedUp(GravityGunInfo info)
    {
        base.OnGravityGunPickedUp(info);

        PhysicsEnabled = true;
    }

    public override void OnGravityGunDropped(GravityGunInfo info)
    {
        base.OnGravityGunDropped(info);
        
        PhysicsEnabled = false;
    }
    
    [Event.Physics.PostStep]
    protected void StayLevel()
    {
        if(IsServer)
            Rotation = Rotation.Angles().WithPitch(0.0f).WithRoll(0.0f).ToRotation();
    }
    
}
