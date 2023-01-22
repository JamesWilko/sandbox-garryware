using System.Collections.Generic;
using Sandbox;

namespace Garryware;

public static class TargetingUtility
{
    private static List<GarrywarePlayer> allPlayers = new();
    private static List<GarrywarePlayer> playersNotLockedIn = new();
    
    private static void UpdateTargetLists()
    { 
        allPlayers.Clear();
        playersNotLockedIn.Clear();
        
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player && player.WasHereForRoundStart)
            {
                allPlayers.Add(player);
                if (!player.HasLockedInResult)
                {
                    playersNotLockedIn.Add(player);
                }
            }
        }
    }
    
    public static GarrywarePlayer GetRandomPlayer()
    {
        UpdateTargetLists();
        return Game.Random.FromList(allPlayers);
    }

    public static GarrywarePlayer GetRandomPlayer(GarrywarePlayer notThisPlayer)
    {
        UpdateTargetLists();

        // If there's only one player then we have to settle for the same person
        if (allPlayers.Count == 1)
            return notThisPlayer;
        
        // Otherwise keep trying for a little while to get a random player who's not been passed in
        int iteration = 0;
        var player = Game.Random.FromList(allPlayers);
        while (player == notThisPlayer && iteration < 16)
        {
            player = Game.Random.FromList(allPlayers);
            ++iteration;
        }
        return player;
    }

    public static GarrywarePlayer GetRandomPlayerStillInPlay()
    {
        UpdateTargetLists();

        if (playersNotLockedIn.Count == 0)
            return null;
        
        return Game.Random.FromList(playersNotLockedIn); 
    }

}
