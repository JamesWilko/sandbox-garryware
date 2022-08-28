using Sandbox;

namespace Garryware;

public class InvertedFirstPersonCamera : FirstPersonCamera
{
    public override void Update()
    {
        base.Update();

        var pawn = Local.Pawn;
        if (pawn == null)
            return;
        
        Rotation = pawn.EyeRotation.Angles().WithRoll(180.0f).ToRotation();
    }
}