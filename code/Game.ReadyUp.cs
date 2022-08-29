using System.Linq;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame
{
    private const float requiredPercentToStartCountdown = 0.5f;
    private const float startGameDelay = 40.0f;
    private const float everyoneReadiedUpStartGameDelay = 5.0f;
    
    public int NumberOfReadyPlayers => Client.All.Count(cl => cl.GetInt(Garryware.Tags.IsReady) > 0);
    private float PercentageOfReadyPlayers => NumberOfReadyPlayers / (float)Client.All.Count;
    
    public bool HasEveryPlayerReadiedUp() => NumberOfReadyPlayers >= Client.All.Count;
    public bool HaveEnoughPlayersReadiedUpToStart() => PercentageOfReadyPlayers > requiredPercentToStartCountdown;
    
    [ConCmd.Server]
    public static void TogglePlayerReadyState()
    {
        var client = ConsoleSystem.Caller;
        if(client == null || !client.IsValid())
            return;

        // Can only un-ready if the game hasn't started counting down to start yet
        var hasAlreadyReadiedUp = client.GetInt(Garryware.Tags.IsReady) > 0;
        if(hasAlreadyReadiedUp && Current.State != GameState.WaitingForPlayers)
            return;
        
        // Toggle the players ready state
        client.SetInt(Garryware.Tags.IsReady, hasAlreadyReadiedUp ? 0 : 1);

        AttemptToStartGame();
    }

    public static void AttemptToStartGame()
    {
        if (Current.IsInState(GameState.WaitingForPlayers))
        {
            Current.RequestTransition(GameState.StartingSoon);
        }
    }
    
}
