using Sandbox;

namespace Garryware;

public class InvertedControlsFirstPersonCamera : FirstPersonCamera
{
    
    public override void BuildInput( InputBuilder input )
    {
        // If we're using the mouse then
        // increase pitch sensitivity
        if ( !input.UsingController )
        {
            input.AnalogLook.pitch *= 1.5f;
        }

        // add the view move, clamp pitch
        input.ViewAngles += input.AnalogLook * -1f;
        input.ViewAngles.pitch = input.ViewAngles.pitch.Clamp( -89, 89 );
        input.ViewAngles.roll = 0;

        // Just copy input as is
        input.InputDirection = input.AnalogMove;
    }
    
}
