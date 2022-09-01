using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Garryware.Entities;
using Sandbox;

namespace Garryware;

// Basic rules to help make games quicker so that we don't have to continuously write logic for basic things like winning or losing on timeout 
[Flags]
public enum MicrogameRules
{
    None = 0,
    WinOnTimeout = 1 << 0, // Everyone who hasn't already lost will win automatically when the game finishes
    LoseOnTimeout = 1 << 1, // Everyone who hasn't already won will lose automatically when the game finishes
    EndEarlyIfEverybodyLockedIn = 1 << 2, // End the game early if everybody locks in a result
    
    DontClearInstructions = 1 << 3,
}

public enum MicrogameRoom
{
    Empty,
    Boxes,
    Platform
}

public abstract class Microgame
{
    public abstract void Setup();
    public abstract void Start();
    public virtual void Tick(){}
    public abstract void Finish();
    public abstract void Cleanup();

    public int MinimumPlayers { get; protected set; } = 1;
    public float GameLength { get; protected set; } = 10.0f;
    public float WarmupLength { get; protected set; } = 1.5f;
    public float CooldownLength { get; protected set; } = 2.5f;
    
    public PlayerAction ActionsUsedInGame { get; protected set; } = PlayerAction.None;
    public MicrogameRules Rules { get; protected set; } = MicrogameRules.None;
    
    /// <summary>
    /// Which rooms can be used for this microgame? If we're not already in one of these rooms when picking this
    /// game, then players will be teleported to the first room specified. 
    /// </summary>
    public MicrogameRoom[] AcceptableRooms { get; protected set; } = { MicrogameRoom.Boxes };
    
    protected GarrywareRoom Room => GarrywareGame.Current.CurrentRoom;
    
    private static readonly List<Entity> TemporaryEntities = new();
    
    private TimeSince timeSinceGameStarted;
    private bool hasGameFinishedEarly;
    private TimeSince timeSinceEarlyFinish;
    
    public virtual bool CanBePlayed()
    {
        return Client.All.Count >= MinimumPlayers;
    }

    public async Task Play()
    {
        // Reset player round states for the new game
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.ResetRound();
            }
        }

        // Clear decals and debris
        Decal.Clear(true, true);
        Debris.Clear();

        // Change to the correct room if we're not in it
        // If we change area then wait a second or two extra so that players can get their bearings
        if (GarrywareGame.Current.ChangeRoom(AcceptableRooms))
        {
            await GameTask.DelayRealtimeSeconds(2.0f);
        }
        
        // Run the microgame logic
        var microgameName = GetType().Name;
        
        Log.Info($"[{microgameName}] Setting up");
        SoundUtility.PlayNewRound(WarmupLength + GameLength);
        Setup();
        GarrywareGame.Current.AvailableActions = ActionsUsedInGame;
        await GameTask.DelayRealtimeSeconds(WarmupLength);
        
        Log.Info($"[{microgameName}] Starting");
        Start();
        
        timeSinceGameStarted = 0;
        GarrywareGame.Current.SetCountdownTimer(GameLength);

        while (!IsGameFinished())
        {
            Tick();
            await GameTask.Yield();
        }
        
        Log.Info($"[{microgameName}] Finished");
        Finish();
        ClearHud();
        ApplyEndOfRoundRules();
        PlayEndOfGameSoundEvents();
        UpdateScores();
        GarrywareGame.Current.AvailableActions = PlayerAction.None;
        await GameTask.DelayRealtimeSeconds(CooldownLength);
        
        Log.Info($"[{microgameName}] Cleaning up");
        Cleanup();
        
        // Automatically clean up as well in case we forget to in the cleanup function
        RemoveAllWeapons();
        CleanupTemporaryEntities();
        ResetColorsInUse();
        hasGameFinishedEarly = false;

        // Reset the decks after the game so we don't have to do it manually per game
        Room.ShuffleDecks();
        CommonEntities.ShuffleDecks();
    }
    
    protected virtual bool IsGameFinished()
    {
        return HasGameTimedOut() || HasGameFinishedEarly() || HasEverybodyLockedInAResult();
    }

    protected bool HasGameTimedOut()
    {
        return timeSinceGameStarted > GameLength;
    }

    protected void EarlyFinish()
    {
        hasGameFinishedEarly = true;
        timeSinceEarlyFinish = 0;
    }

    protected bool HasGameFinishedEarly()
    {
        return hasGameFinishedEarly && timeSinceEarlyFinish > 0.5f;
    }

    protected bool HasEverybodyLockedInAResult()
    {
        if (!Rules.HasFlag(MicrogameRules.EndEarlyIfEverybodyLockedIn))
            return false;
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && !player.HasLockedInResult)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Show an instruction popup to all players that goes away after a short time.
    /// </summary>
    protected void ShowInstructions(string text, float? displayTime = null)
    {
        float duration = displayTime ?? (GameLength + WarmupLength);
        GameEvents.NewInstructions(text, duration);
    }

    /// <summary>
    /// Clear all microgame related things off the hud except for the result
    /// </summary>
    protected void ClearHud()
    {
        if (!Rules.HasFlag(MicrogameRules.DontClearInstructions))
            GameEvents.ClearInstructions();
        
        GarrywareGame.Current.ClearCountdownTimer();
        GarrywareGame.Current.AvailableActions = PlayerAction.None;
    }

    /// <summary>
    /// Remove all weapons from all players, and from the world.
    /// </summary>
    protected void RemoveAllWeapons()
    {
        // Find any weapons that were dropped into the world and delete them
        foreach (var weapon in Entity.All.OfType<BaseCarriable>())
        {
            weapon.Delete();
        }
        
        // Remove weapons from all players
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.RemoveWeapons();
            }
        }
    }

    /// <summary>
    /// Give a specific weapon to the specified players.
    /// </summary>
    protected List<T> GiveWeapon<T>(To giveToWho) where T : BaseCarriable, new()
    {
        var weapons = new List<T>();
        foreach (var client in giveToWho)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                var weapon = new T();
                weapons.Add(weapon);
                player.Inventory.Add(weapon, true);
            }
        }
        return weapons;
    }
    
    /// <summary>
    /// Flag an entity for automatic clean up at the end of the microgame.
    /// </summary>
    protected void AutoCleanup(Entity ent)
    {
        TemporaryEntities.Add(ent);
    }

    /// <summary>
    /// Clean up all temporary entities that were spawned.
    /// </summary>
    protected void CleanupTemporaryEntities()
    {
        foreach (var ent in TemporaryEntities)
        {
            ent.Delete();
        }
        TemporaryEntities.Clear();
    }

    private readonly List<GameColor> colorsInUse = new();
    
    /// <summary>
    /// Get a random color from the colors deck and add it to the list of in-use colors
    /// </summary>
    protected GameColor GetRandomColor()
    {
        return UseColor(CommonEntities.ColorsDeck.Next());
    }

    /// <summary>
    /// Specify a specific color to be used, and add it to the randomization list in case we want to access it later
    /// </summary>
    protected GameColor UseColor(GameColor gameColor)
    {
        colorsInUse.Add(gameColor);
        return gameColor;
    }

    /// <summary>
    /// Get a random color that's already been used this
    /// </summary>
    protected GameColor GetRandomColorAlreadyInUse()
    {
        return Random.Shared.FromList(colorsInUse);
    }

    /// <summary>
    /// Reset the list of colors in use.
    /// </summary>
    protected void ResetColorsInUse()
    {
        colorsInUse.Clear();
    }
    
    private void ApplyEndOfRoundRules()
    {
        // Automatically cause all remaining players who haven't been locked in already to win
        if (Rules.HasFlag(MicrogameRules.WinOnTimeout))
        {
            foreach (var player in Client.All.Select(client => client.Pawn).OfType<GarrywarePlayer>().Where(player => !player.HasLockedInResult))
            {
                player.FlagAsRoundWinner();
            }
        }
        
        // Automatically cause all remaining players who haven't been locked in already to lose
        if (Rules.HasFlag(MicrogameRules.LoseOnTimeout))
        {
            foreach (var player in Client.All.Select(client => client.Pawn).OfType<GarrywarePlayer>().Where(player => !player.HasLockedInResult))
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    private void PlayEndOfGameSoundEvents()
    {
        var winners = Client.All.Where(client => client.Pawn is GarrywarePlayer player && player.HasWonRound).ToArray();
        var losers = Client.All.Where(client => client.Pawn is GarrywarePlayer player && player.HasLostRound).ToArray();

        bool everyoneWon = GarrywareGame.Current.NumConnectedClients > 2 && winners.Length == GarrywareGame.Current.NumConnectedClients;
        bool everyoneLost = losers.Length == GarrywareGame.Current.NumConnectedClients;

        if (everyoneWon)
        {
            SoundUtility.PlayEveryoneWon();
        }
        else if (everyoneLost)
        {
            SoundUtility.PlayEveryoneLost();
        }
        else
        {
            SoundUtility.PlayWinRound(To.Multiple(winners));
            SoundUtility.PlayLoseRound(To.Multiple(losers));
        }
    }

    // @todo: update this later on to immediately give points when the player wins or loses
    // @todo: fix lots of ugly get and set ints on a magic variable name
    private void UpdateScores()
    {
        var winners = Client.All.Where(client => client.Pawn is GarrywarePlayer player && player.HasWonRound).ToArray();
        var losers = Client.All.Where(client => client.Pawn is GarrywarePlayer player && player.HasLostRound).ToArray();
        var pointsPlacing = new List<int>();
        
        // Give all winners a point and update their streak
        foreach (var client in winners)
        {
            var newPoints = client.GetInt(Tags.Points) + 1;
            var oldStreak = client.GetInt(Tags.Streak);
            var newStreak = oldStreak + 1;

            client.SetInt(Tags.Points, newPoints);
            client.SetInt(Tags.Streak, newStreak);
            if (newStreak > oldStreak)
            {
                client.SetInt(Tags.MaxStreak, newStreak);
            }
            
            pointsPlacing.AddUnique(newPoints);
        }
        
        // Reset the streak on all losers
        foreach (var client in losers)
        {
            client.SetInt(Tags.Streak, 0);
            
            pointsPlacing.AddUnique(client.GetInt(Tags.Points));
        }
        
        // Sort points out into their points order and assign a place to each player based on their points 
        pointsPlacing.Sort();
        pointsPlacing.Reverse();
        foreach (var client in Client.All)
        {
            int place = pointsPlacing.IndexOf(client.GetInt(Tags.Points)) + 1;
            client.SetInt(Tags.Place, place);
        }

    }

}
