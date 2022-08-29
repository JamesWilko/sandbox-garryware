using System.Linq;
using Core.StateMachine;
using Garryware.Entities;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame : Sandbox.Game
{
    public new static GarrywareGame Current { get; private set; }
    
    private readonly ShuffledDeck<Microgame> microgamesDeck = new();
    
    [Net] public int NumConnectedClients { get; set; }
    
    [Net] public bool IsCountdownTimerEnabled { get; private set; }
    [Net] public TimeUntil TimeUntilCountdownExpires { get; private set; }
    private TimeUntil TimeUntilGameStarts { get; set; }
    
    [Net] public int CurrentRound { get; private set; }

    [ConVar.Replicated("gw_max_rounds")]
    public static int MaxRounds { get; set; } = 40;

    [Net, Change(nameof(OnAvailableActionsChanged))] public PlayerAction AvailableActions { get; set; }
    
    public GarrywareGame()
    {
        Current = this;
        CommonEntities.Precache();
        
        if (IsServer)
        {
            _ = new GameEvents();
            _ = new GarrywareHud();
        }

        // Setup state control
        AddExitStateController(GameState.WaitingForPlayers, HaveEnoughPlayersReadiedUp);
        AddEnterStateObserver(GameState.StartingSoon, OnEnterStartingSoonState);
        AddEnterStateObserver(GameState.Instructions, OnEnterInstructionsState);
        AddEnterStateObserver(GameState.Playing, OnEnterPlayingState);
        AddEnterStateObserver(GameState.GameOver, OnEnterGameOverState);
        
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
    
    // Only start the game once we've got enough players readied up to start
    private TransitionResponse HaveEnoughPlayersReadiedUp(TransitionArgs<GameState> args)
    {
        return HaveEnoughPlayersReadiedUpToStart() ? TransitionResponse.Allow : TransitionResponse.Block;
    }
    
    // Once enough people have readied up then start a countdown until the game starts and everybody is forced to play
    private void OnEnterStartingSoonState(TransitionArgs<GameState> args)
    {
        var startDelay = HasEveryPlayerReadiedUp() ? everyoneReadiedUpStartGameDelay : startGameDelay;
        SetCountdownTimer(startDelay);
        TimeUntilGameStarts = startDelay;
    }

    [Event.Tick.Server]
    private void StartGameTick()
    {
        if (!IsInState(GameState.StartingSoon))
            return;

        // If every connected player readies up then cut the countdown down
        if (TimeUntilGameStarts > everyoneReadiedUpStartGameDelay && HasEveryPlayerReadiedUp())
        {
            SetCountdownTimer(everyoneReadiedUpStartGameDelay);
            TimeUntilGameStarts = everyoneReadiedUpStartGameDelay;
        }
        
        // Start the game!
        if (TimeUntilGameStarts < 0)
        {
            RequestTransition(GameState.Instructions);
        }
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

            CurrentRound++;
            await microgame.Play();
        }
        while (CanContinuePlaying());

        RequestTransition(GameState.GameOver);
    }
    
    private bool CanContinuePlaying()
    {
        // Have we reached the round limit?
        // Has somebody won?
        // Has everybody left?
        // @todo
        return CurrentRound < MaxRounds;
    }

    private void RefreshAvailableMicrogames()
    {
        Log.Info("Refreshing available microgames...");
        microgamesDeck.Clear();
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

    private async void OnEnterGameOverState(TransitionArgs<GameState> args)
    {
        const float returnToLobbySeconds = 30.0f;
        
        await GameServices.EndGameAsync();
        
        SetCountdownTimer(returnToLobbySeconds);
        await GameTask.DelayRealtimeSeconds(returnToLobbySeconds);
        
        // Kick everybody out of the game
        Client.All.ToList().ForEach( cl => cl.Kick() );
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
    }
    
    private void OnAvailableActionsChanged(PlayerAction oldActions, PlayerAction newActions)
    {
        GameEvents.UpdatePlayerActions(newActions);
    }
    
}

