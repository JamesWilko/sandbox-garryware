using System.Collections.Generic;
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
    [Net] public int NumberOfWinners { get; set; }
    [Net] public int NumberOfLosers { get; set; }
    
    [Net] public bool IsCountdownTimerEnabled { get; private set; }
    [Net] public TimeUntil TimeUntilCountdownExpires { get; private set; }
    private TimeUntil TimeUntilGameStarts { get; set; }
    
    [Net] public int CurrentRound { get; private set; }

    [ConVar.Replicated("gw_max_rounds")]
    public static int MaxRounds { get; set; } = 40;

    [Net] public PlayerAction AvailableActions { get; set; }
    
    [Net] public GarrywareRoom CurrentRoom { get; set; }
    private int playerCountWhenRoomLastChanged;
    
    [ConVar.Server("gw_skip_tutorial")]
    public static bool SkipTutorial { get; set; }
    
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
        AddExitStateObserver(GameState.StartingSoon, OnExitStartingSoonState);
        AddEnterStateObserver(GameState.Instructions, OnEnterInstructionsState);
        AddEnterStateObserver(GameState.Playing, OnEnterPlayingState);
        AddEnterStateObserver(GameState.GameOver, OnEnterGameOverState);
        
        AddExitStateController(GameState.Dev, args => TransitionResponse.Block);
        
        AvailableActions = PlayerAction.ReadyUp;
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
        
        CurrentRoom = Entity.All.OfType<GarrywareRoom>().FirstOrDefault(room => room.Contents == MicrogameRoom.Boxes && room.Size == RoomSize.Large);
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
    
    private void OnExitStartingSoonState(TransitionArgs<GameState> args)
    {
        AvailableActions = PlayerAction.None;
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
    
    private async void OnEnterInstructionsState(TransitionArgs<GameState> args)
    {
        ClearCountdownTimer();

        if (!SkipTutorial)
        {
            // Play the tutorial game
            var tutorialGame = new Microgames.TutorialGame();
            await tutorialGame.Play();
        }

        // Then move onto playing for real
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
        // Keep playing until we reach the number of rounds set in the lobby
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
        GameEvents.TriggerGameOver();
        
        SetCountdownTimer(returnToLobbySeconds);
        await GameTask.DelayRealtimeSeconds(returnToLobbySeconds);
        
        // Kick everybody out of the game
        Client.All.ToList().ForEach(cl => cl.Kick());
    }
    
    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // Spawn the player in
        var player = new GarrywarePlayer(client);
        player.Respawn();
        client.Pawn = player;
    }

    public override void MoveToSpawnpoint(Entity pawn)
    {
        var spawnpoint = CurrentRoom.SpawnPointsDeck.Next();
        if (spawnpoint == null)
        {
            Log.Warning($"Couldn't find spawnpoint for {pawn}!");
            return;
        }
        pawn.Transform = spawnpoint.Transform;
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
    
    /// <summary>
    /// Attempt to swap to a room that is acceptable for the microgame to take place in.
    /// Returns true if a room swap occured, or false if players didn't teleport anywhere.
    /// </summary>
    /// <param name="acceptableRooms">A list of rooms the microgame can take place in. If not currently in one of them,
    /// then the players will be teleported to the first room in the array.</param>
    public bool ChangeRoom(MicrogameRoom[] acceptableRooms)
    {
        // Check if we're already in one of the desired rooms, if we are then don't bother swapping since this room is good enough
        // Only do this if the player count hasn't changed in case we need to swap to a bigger or smaller room 
        if (playerCountWhenRoomLastChanged == Client.All.Count)
        {
            foreach (var roomContent in acceptableRooms)
            {
                if (CurrentRoom.Contents == roomContent)
                {
                    return false;
                }
            }
        }

        // Change to the first room in the list if we're not already in a room that is acceptable
        var targetRoom = acceptableRooms.First();
        var size = GetAppropriateRoomSizeForPlayerCount();
        var newRoom = Entity.All.OfType<GarrywareRoom>().FirstOrDefault(room => room.Contents == targetRoom && room.Size == size);
        Assert.True(newRoom != null, $"No room for {targetRoom} and size {size} was found in the map!");
        
        // Don't teleport if the room is the same even after a player count change
        if (newRoom == CurrentRoom)
        {
            return false;
        }
        CurrentRoom = newRoom;

        // Teleport all players to the new room
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.TeleportTo(CurrentRoom.SpawnPointsDeck.Next().Transform);
            }
        }

        playerCountWhenRoomLastChanged = Client.All.Count;
        return true;
    }

    private RoomSize GetAppropriateRoomSizeForPlayerCount()
    {
        var playerCount = Client.All.Count;
        
        if (playerCount >= 12) return RoomSize.Large;
        if (playerCount <= 6) return RoomSize.Small;
        return RoomSize.Medium;
    }
    
    public void UpdateWinLoseCounts()
    {
        int winners = 0;
        int losers = 0;
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && player.HasLockedInResult)
            {
                winners += player.HasWonRound ? 1 : 0;
                losers += player.HasLostRound ? 1 : 0;
            }
        }

        NumberOfWinners = winners;
        NumberOfLosers = losers;
    }
    
    
    private readonly Curve sizeAtDistanceCurve = new(new List<Curve.Frame>()
    {
        new(0, 0.5f),
        new(200, 0.08f),
        new(1000, 0.04f),
    });

    private readonly Curve fontSizeAtDistanceCurve = new(new List<Curve.Frame>()
    {
        new(0, 256),
        new(200, 32),
        new(1000, 20),
    });
    
    public override void RenderHud()
    {
        base.RenderHud();

        var draw = Render.Draw2D;
        draw.FontFamily = "Poppins";
        draw.FontWeight = 600;
        
        // @todo: this is really shit, but it works without any fuss so it's fine for now
        foreach (var ent in Entity.All)
        {
            if (ent is BreakableProp prop && prop.ShowWorldText)
            {
                float distance = Local.Pawn.EyePosition.Distance(prop.Position);
                float size = sizeAtDistanceCurve.Evaluate(distance) * Screen.Height;
                
                var screenPosition = prop.WorldSpaceBounds.Center.ToScreen();
                var rect = new Rect(screenPosition.x * Screen.Width - size * 0.5f, screenPosition.y * Screen.Height - size * 0.5f, size, size);
                draw.Color = Color.Black.WithAlpha(0.8f);
                draw.Box(rect);

                draw.FontSize = fontSizeAtDistanceCurve.Evaluate(distance);
                draw.Color = Color.White;
                draw.DrawText(rect, prop.WorldText);
            }
        }
        
    }
    
}
