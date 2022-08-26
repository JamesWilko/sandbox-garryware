using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class GameOver : Panel
{

    public GameOver()
    {
        Add.Label("Temporary Game Over Screen! Returning to lobby soon...");
    }

    public override void Tick()
    {
        base.Tick();

        Style.Opacity = GarrywareGame.Current.State == GameState.GameOver ? 1 : 0;
    }
}
