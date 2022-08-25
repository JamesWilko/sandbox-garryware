using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Garryware;

// Basic rules to help make games quicker so that we don't have to continuously write logic for basic things like winning or losing on timeout 
public enum MicrogameRules
{
    None = 0,
    WinOnTimeout = 1 << 0, // Everyone who hasn't already lost will win automatically when the game finishes
    LoseOnTimeout = 1 << 1, // Everyone who hasn't already won will lose automatically when the game finishes
}

public abstract class Microgame
{
    public abstract void Setup();
    public abstract void Start();
    public virtual void Tick(){}
    public abstract void Finish();
    public abstract void Cleanup();

    public int MinimumPlayers { get; protected set; } = 2;
    public float GameLength { get; protected set; } = 10.0f;
    public float WarmupLength { get; protected set; } = 1.5f;
    public float CooldownLength { get; protected set; } = 3.0f;
    
    public PlayerAction ActionsUsedInGame { get; protected set; } = PlayerAction.None;
    public MicrogameRules Rules { get; protected set; } = MicrogameRules.None;

    private static readonly List<Entity> TemporaryEntities = new();
    
    private TimeSince timeSinceGameStarted;
    private bool hasGameFinishedEarly;
    private TimeSince timeSinceEarlyFinish;

    private const float defaultInstructionsDisplayTime = 6.0f;
    
    public virtual bool CanBePlayed()
    {
        // @todo: put this back in after testing
        // return Client.All.Count >= MinimumPlayers;
        return true;
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
        
        // Run the microgame logic
        var microgameName = GetType().Name;
        
        Log.Info($"[{microgameName}] Setting up");
        SoundUtility.PlayNewRound();
        Setup();
        GarrywareGame.Current.AvailableControls = ActionsUsedInGame;
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
        GarrywareGame.Current.AvailableControls = PlayerAction.None;
        GarrywareGame.Current.ClearCountdownTimer();
        ApplyEndOfRoundRules();
        PlayEndOfGameSoundEvents();
        await GameTask.DelayRealtimeSeconds(CooldownLength);
        
        Log.Info($"[{microgameName}] Cleaning up");
        Cleanup();
        
        // Automatically clean up as well in case we forget to in the cleanup function
        RemoveAllWeapons();
        CleanupTemporaryEntities();
        CommonEntities.ShuffleWorldEntityDecks(); // Reset the decks after the game so we don't have to do it manually per game
        hasGameFinishedEarly = false;
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
    protected void ShowInstructions(string text, float displayTime = defaultInstructionsDisplayTime)
    {
        GarrywareGame.Current.ShowInstructions(text, displayTime);
    }

    /// <summary>
    /// Remove all weapons from all players, and from the world.
    /// </summary>
    protected void RemoveAllWeapons()
    {
        // Find any weapons that were dropped into the world and delete them
        foreach (var weapon in Entity.All.OfType<Weapon>())
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
    protected void GiveWeapon<T>(To giveToWho) where T : Weapon, new()
    {
        foreach (var client in giveToWho)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Inventory.Add(new T(), true);
            }
        }
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

}
