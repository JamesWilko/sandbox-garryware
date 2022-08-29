using Sandbox;

namespace Garryware;

public partial class GameEvents : BaseNetworkable
{
    
    public delegate void InstructionsDelegate(string text, float displayTime);
    public static event InstructionsDelegate OnNewInstructions;
    public static event InstructionsDelegate OnClearInstructions;

    public delegate void RoundResultDelegate(Client player, RoundResult result);
    public static event RoundResultDelegate OnPlayerLockedInResult;
    public static event RoundResultDelegate OnPlayerWon;
    public static event RoundResultDelegate OnPlayerLost;
    
    [ClientRpc]
    public static void NewInstructions(string text, float displayTime)
    {
        OnNewInstructions?.Invoke(text, displayTime);
    }

    [ClientRpc]
    public static void ClearInstructions()
    {
        OnClearInstructions?.Invoke(string.Empty, -1);
    }

    [ClientRpc]
    public static void PlayerLockedInResult(Client player, RoundResult result)
    {
        switch (result)
        {
            case RoundResult.Won:
                OnPlayerLockedInResult?.Invoke(player, result);
                OnPlayerWon?.Invoke(player, result);
                break;
            case RoundResult.Lost:
                OnPlayerLockedInResult?.Invoke(player, result);
                OnPlayerLost?.Invoke(player, result);
                break;
        }
    }

}
