using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class GameOver : Panel
{

    public GameOver()
    {
        Add.Label("#ui.gameover");
    }

    public override void Tick()
    {
        base.Tick();

        Style.Opacity = GarrywareGame.Current.State == GameState.GameOver ? 1 : 0;
    }
}
