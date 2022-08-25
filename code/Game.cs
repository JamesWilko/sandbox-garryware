using Core.StateMachine;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame : Sandbox.Game
{
    public new static GarrywareGame Current { get; private set; }

    private const int MaxGamesToPlay = 30;
    private const int PointsToWin = 15;
    private const int MaxRepeatsPerMicrogame = 2;

    private const float EveryoneConnectedStartGameDelay = 10.0f;
    
    private int microgamesPlayed;
    private readonly ShuffledDeck<Microgame> microgamesDeck = new();
    
    [Net] public int NumConnectedClients { get; set; }
    
    [Net] private TimeSince TimeSinceEverybodyConnected { get; set; }
    
    [Net] public bool IsCountdownTimerEnabled { get; private set; }
    [Net] public TimeUntil TimeUntilCountdownExpires { get; private set; }
    
    public delegate void ShowInstructionsDelegate(string text, float displayTime);
    public event ShowInstructionsDelegate OnNewInstructions;

    public delegate void RoundResultDelegate(RoundResult result);
    public event RoundResultDelegate OnRoundResult;
    
    [Net, Change(nameof(OnAvailableControlsChanged))] public PlayerAction AvailableControls { get; set; }
    public delegate void MicrogameControlsDelegate(PlayerAction availableActions);
    public event MicrogameControlsDelegate OnAvailableControlsUpdated;
    
    public GarrywareGame()
    {
        Current = this;
        CommonEntities.Precache();
        
        if (IsServer)
        {
            _ = new GarrywareHud();
        }
        _ = new MusicController();
        
        // Setup state control
        AddExitStateController(GameState.WaitingForPlayers, HasEverybodyConnected);
        AddEnterStateObserver(GameState.StartingSoon, OnEnterStartingSoonState);
        AddEnterStateObserver(GameState.Instructions, OnEnterInstructionsState);
        AddEnterStateObserver(GameState.Playing, OnEnterPlayingState);
        
        AddExitStateController(GameState.Dev, args => TransitionResponse.Block);
        
        Enable();
    }

    public void SetCountdownTimer(float seconds)
    {
        using (LagCompensation())
        {
            TimeUntilCountdownExpires = seconds;
            IsCountdownTimerEnabled = true;
        }
    }

    public void ClearCountdownTimer()
    {
        IsCountdownTimerEnabled = false;
    }
    
    public override void PostLevelLoaded()
    {
        base.PostLevelLoaded();
        
        // Cache all the map entities that microgames may be accessing
        CommonEntities.PrecacheWorldEntities();
    }
    
    // Only start the game once we've got everybody in the game and able to run around
    private TransitionResponse HasEverybodyConnected(TransitionArgs<GameState> args)
    {
        return NumConnectedClients >= Client.All.Count ? TransitionResponse.Allow : TransitionResponse.Block;
    }
    
    // When we enter the starting soon state then wait a short time for everyone to be actually in the game and running around
    private async void OnEnterStartingSoonState(TransitionArgs<GameState> args)
    {
        using (LagCompensation())
        {
            TimeSinceEverybodyConnected = 0;
        }
        SetCountdownTimer(EveryoneConnectedStartGameDelay);

        await GameTask.DelayRealtimeSeconds(EveryoneConnectedStartGameDelay);
        RequestTransition(GameState.Instructions);
    }

    private void OnEnterInstructionsState(TransitionArgs<GameState> args)
    {
        ClearCountdownTimer();
        
        // @todo: tutorial microgame
        // For now we'll just skip straight to playing
        RequestTransition(GameState.Playing);
    }
    
    private async void OnEnterPlayingState(TransitionArgs<GameState> args)
    {
        // Determine which games we'll be able to play
        RefreshAvailableMicrogames();

        // Pick a random microgame from the deck
        // Play the game
        // Check if we can continue playing and repeat
        do
        {
            var microgame = microgamesDeck.Next();
            Assert.NotNull(microgame);
            
            await microgame.Play();
            microgamesPlayed++;
        }
        while (CanContinuePlaying());

        RequestTransition(GameState.GameOver);
    }
    
    private bool CanContinuePlaying()
    {
        // Have we reached the round limit?
        // Has somebody won?
        // Has everybody left?
        return microgamesPlayed < MaxGamesToPlay;
    }

    private void RefreshAvailableMicrogames()
    {
        Log.Info("Refreshing available microgames...");
        foreach (var microgame in MicrogamesList.Microgames)
        {
            if (microgame.CanBePlayed())
            {
                microgamesDeck.Add(microgame);
                Log.Info($"    Including microgame: {microgame}");
            }
            else
            {
                Log.Info($"    Excluded microgame: {microgame}");
            }
        }
        Assert.True(microgamesDeck.Count > 0);
    }
    
    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // Spawn the player in
        var player = new GarrywarePlayer(client);
        player.Respawn();
        client.Pawn = player;
    }

    public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
    {
        base.ClientDisconnect(cl, reason);
        NumConnectedClients--;
    }

    public override void DoPlayerSuicide(Client cl)
    {
        // Do nothing
        // Players aren't allowed to suicide in microgames
    }

    public override void OnClientActive(Client client)
    {
        base.OnClientActive(client);
        NumConnectedClients++;
        
        // Attempt to advance the game state
        RequestTransition(GameState.StartingSoon);
    }
    
    [ClientRpc]
    public void ShowInstructions(string text, float displayTime = 3.0f)
    {
        OnNewInstructions?.Invoke(text, displayTime);
    }
    
    public void ShowRoundResult(RoundResult result)
    {
        OnRoundResult?.Invoke(result);
    }
    
    private void OnAvailableControlsChanged(PlayerAction oldControls, PlayerAction newControls)
    {
        OnAvailableControlsUpdated?.Invoke(newControls);
    }
    
    [ConCmd.Server("gw_dev")]
    public static void EnableDevMode()
    {
        Current?.RequestTransition(GameState.Dev);
        
        foreach (var client in To.Everyone)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Inventory.Add(new Pistol(), true);
            }
        }
    }

}

