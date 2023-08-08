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
                Style.Display = DisplayMode.Flex;
                waiting.SetText("#ui.waiting-for-players");
                playersConnected.SetText(string.Format("{0}/{1}", GarrywareGame.Current.NumberOfReadyPlayers, GarrywareGame.Current.NumberOfReadiesNeededToStart)); // @localization
                break;
            case GameState.StartingSoon:
                Style.Display = DisplayMode.Flex;
                waiting.SetText("#ui.starting-soon");
                playersConnected.SetText(string.Empty);
                break;
            case GameState.Playing:
                if (Game.LocalPawn is GarrywarePlayer player && !player.WasHereForRoundStart)
                {
                    Style.Display = DisplayMode.Flex;
                    waiting.SetText("#ui.joined-in-progress");
                    playersConnected.SetText(string.Empty);
                }
                else
                {
                    Style.Display = DisplayMode.None;
                }
                break;
            case GameState.WrongMap:
                Style.Display = DisplayMode.Flex;
                waiting.SetText("#ui.wrong-map");
                playersConnected.SetText(string.Empty);
                break;
            default:
                Style.Display = DisplayMode.None;
                break;
        }
    }
}
