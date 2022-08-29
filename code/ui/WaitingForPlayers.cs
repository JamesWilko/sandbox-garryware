using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

// @todo: actually good
public class WaitingForPlayers : Panel
{
    private readonly Label waiting;
    private readonly Label playersConnected;
    
    private const string waitingForPlayersText = "Game will start once enough players are ready...   ";
    private const string connectedPlayersText = "{0}/{1}";
    private const string startingSoonText = "Starting shortly!";
    
    public WaitingForPlayers()
    {
        waiting = Add.Label(waitingForPlayersText);
        playersConnected = Add.Label(connectedPlayersText);
    }

    public override void Tick()
    {
        base.Tick();

        switch (GarrywareGame.Current.State)
        {
            case GameState.WaitingForPlayers:
                Style.Opacity = 1.0f;
                waiting.SetText(waitingForPlayersText);
                playersConnected.SetText(string.Format(connectedPlayersText, GarrywareGame.Current.NumberOfReadyPlayers, GarrywareGame.Current.NumberOfReadiesNeededToStart));
                break;
            case GameState.StartingSoon:
                Style.Opacity = 1.0f;
                waiting.SetText(startingSoonText);
                playersConnected.SetText(string.Empty);
                break;
            default:
                Style.Opacity = 0.0f;
                break;
        }
    }
}
