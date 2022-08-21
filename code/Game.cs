using Core.StateMachine;
using Sandbox;


namespace Garryware;

public partial class GarrywareGame : Sandbox.Game
{
    public new static GarrywareGame Current = Game.Current as GarrywareGame;
    
    private const int MaxGamesToPlay = 30;
    private const int PointsToWin = 15;
    private const int MaxRepeatsPerMicrogame = 2;

    private const float EveryoneConnectedStartGameDelay = 10.0f;
    
    private int clientsJoined;
    private int microgamesPlayed;
    private ShuffledDeck<Microgame> microgamesDeck = new();

    private StateMachine<GameState> GameStateMachine { get; set; }
    [Net] private GameState ReplicatedGameState { get; set; }
    
    public GarrywareGame()
    {
        CommonEntities.Precache();
        
        if (IsServer)
        {
            _ = new GarrywareHud();
        }
        
        _ = new MusicController();
        
        GameStateMachine = new StateMachine<GameState>();
        GameStateMachine.AddExitStateController(GameState.WaitingForPlayers, HasEverybodyConnected);
        GameStateMachine.AddEnterStateObserver(GameState.StartingSoon, OnEnterStartingSoonState);
        GameStateMachine.AddEnterStateObserver(GameState.Instructions, OnEnterInstructionsState);
        GameStateMachine.AddEnterStateObserver(GameState.Playing, OnEnterPlayingState);
        
        GameStateMachine.AddExitStateController(GameState.Dev, args => TransitionResponse.Block);
        
        GameStateMachine.Enable();
    }
    
    [Event.Tick.Server]
    private void ServerTick()
    {
        GameStateMachine?.Update();
        ReplicatedGameState = GameStateMachine?.State ?? GameState.NotRunning;
    }

    // Only start the game once we've got everybody in the game and able to run around
    private TransitionResponse HasEverybodyConnected(TransitionArgs<GameState> args)
    {
        return clientsJoined >= Client.All.Count ? TransitionResponse.Allow : TransitionResponse.Block;
    }
    
    // When we enter the starting soon state then wait a short time for everyone to be actually in the game and running around
    private async void OnEnterStartingSoonState(TransitionArgs<GameState> args)
    {
        // Cache all the map entities that microgames may be accessing
        Microgame.FirstTimeSetup();
        
        await GameTask.DelayRealtimeSeconds(EveryoneConnectedStartGameDelay);
        GameStateMachine.RequestTransition(GameState.Instructions);
    }

    private void OnEnterInstructionsState(TransitionArgs<GameState> args)
    {
        // @todo: tutorial microgame
        // For now we'll just skip straight to playing
        GameStateMachine.RequestTransition(GameState.Playing);
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

        GameStateMachine.RequestTransition(GameState.GameOver);
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
        clientsJoined--;
    }

    public override void DoPlayerSuicide(Client cl)
    {
        // Do nothing
        // Players aren't allowed to suicide in microgames
    }

    public override void OnClientActive(Client client)
    {
        base.OnClientActive(client);
        clientsJoined++;
        
        // Attempt to advance the game state
        GameStateMachine.RequestTransition(GameState.StartingSoon);
    }


    [ConCmd.Server("gw_dev")]
    public static void EnableDevMode()
    {
        Current.GameStateMachine?.RequestTransition(GameState.Dev);
        
        foreach (var client in To.Everyone)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Inventory.Add(new Pistol(), true);
            }
        }
    }
    
    
}

