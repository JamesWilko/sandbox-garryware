using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

// @todo: actually good
public class WaitingForPlayers : Panel
{
    private readonly Label waiting;
    private readonly Label playersConnected;
    
    public WaitingForPlayers()
    {
        waiting = Add.Label("#ui.waiting-for-players");
        playersConnected = Add.Label();
    }

    public override void Tick()
    {
        base.Tick();

        switch (GarrywareGame.Current.State)
        {
            case GameState.WaitingForPlayers:
                Style.Opacity = 1.0f;
                waiting.SetText("#ui.waiting-for-players");
                playersConnected.SetText(string.Format("{0}/{1}", GarrywareGame.Current.NumberOfReadyPlayers, GarrywareGame.Current.NumberOfReadiesNeededToStart)); // @localization
                break;
            case GameState.StartingSoon:
                Style.Opacity = 1.0f;
                waiting.SetText("#ui.starting-soon");
                playersConnected.SetText(string.Empty);
                break;
            default:
                Style.Opacity = 0.0f;
                break;
        }
    }
}
