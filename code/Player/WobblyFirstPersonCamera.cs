using Sandbox;

namespace Garryware;

public class WobblyFirstPersonCamera : FirstPersonCamera
{
    
    public override void BuildInput( InputBuilder input )
    {
        var pawn = Local.Pawn;
        if (pawn == null)
            return;
        
        float wobbleX = (Noise.Perlin(Time.Tick + pawn.Position.x) - 0.5f) * 1.3f;
        float wobbleY = (Noise.Perlin(Time.Tick + pawn.Position.y) - 0.5f) * 1.3f;
        
        // If we're using the mouse then
        // increase pitch sensitivity
        if ( !input.UsingController )
        {
            input.AnalogLook.pitch *= 1.5f;
        }

        // add the view move, clamp pitch
        input.ViewAngles += input.AnalogLook;
        input.ViewAngles.pitch += wobbleY;
        input.ViewAngles.yaw += wobbleX;
        input.ViewAngles.pitch = input.ViewAngles.pitch.Clamp( -89, 89 );
        input.ViewAngles.roll = 0;

        // Just copy input as is
        input.InputDirection = input.AnalogMove;
    }
    
}
