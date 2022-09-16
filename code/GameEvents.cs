﻿using Sandbox;

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

    public static event System.Action<RoundStat, Client> ClientStatReceived;
    public static event System.Action<RoundStat, int> IntegerStatReceived;

    public delegate void MicrogameUiClassDelegate(string className);
    public static event MicrogameUiClassDelegate NewMicrogameUi;
    public static event MicrogameUiClassDelegate ClearMicrogameUi;
    
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

    [ClientRpc]
    public static void SendClientStat(RoundStat stat, Client subject)
    {
        ClientStatReceived?.Invoke(stat, subject);
    }

    [ClientRpc]
    public static void SendIntegerStat(RoundStat stat, int value)
    {
        IntegerStatReceived?.Invoke(stat, value);
    }

    [ClientRpc]
    public static void ShowMicrogameUi(string className)
    {
        NewMicrogameUi?.Invoke(className);
    }
    
    [ClientRpc]
    public static void RemoveMicrogameUi()
    {
        ClearMicrogameUi?.Invoke(string.Empty);
    }
    
}
