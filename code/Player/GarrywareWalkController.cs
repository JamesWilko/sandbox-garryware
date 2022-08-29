using Sandbox;

namespace Garryware;

public class GarrywareWalkController : WalkController
{
    public override void Simulate()
    {
        base.Simulate();

        // Set player's ready up state
        if (Input.Pressed(InputButton.Flashlight))
        {
            GarrywareGame.TogglePlayerReadyState();
        }
    }
}
