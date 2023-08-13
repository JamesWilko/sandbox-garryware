using System;
using System.Collections.Generic;
using System.Linq;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class FewestPlayersOnPlatform : Microgame
{
    private readonly Dictionary<Entity, int> numPlayersOnPlatforms = new();
    private List<Platform> platforms = new();
    
    public FewestPlayersOnPlatform()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Empty };
        GameLength = 5;
        MinimumPlayers = 3;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.fewest-players-on-platform");
        GiveWeapon<RocketLauncher>(To.Everyone);
        
        int maxPlatforms = Game.Clients.Count switch
        {
            < 5 => 2,
            < 8 => 3,
            < 12 => 5,
            < 20 => 6,
            _ => 8
        };
        int numPlatforms = GetRandomAdjustedClientCount(0.3f, 0.6f, 1, maxPlatforms);
        for (int i = 0; i < numPlatforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 5f,
                GameColor = CommonEntities.ColorsDeck.Next()
            };
            platforms.Add(platform);
            AutoCleanup(platform);
        }
    }

    public override void Finish()
    {
        // Count how many players on each platform
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player && player.GroundEntity != null && !player.GroundEntity.IsWorld)
            {
                if (!numPlayersOnPlatforms.ContainsKey(player.GroundEntity))
                {
                    numPlayersOnPlatforms.Add(player.GroundEntity, 0);
                }

                numPlayersOnPlatforms[player.GroundEntity] += 1;
            }
        }
        
        // Check if everybody lost
        if(numPlayersOnPlatforms.Count == 0)
            return;
        
        // Get the lowest number of players on a platform
        int fewestAmountOfPlayers = numPlayersOnPlatforms.MinBy(kvp => kvp.Value).Value;
        
        // Check all platforms and award the win to any player whose on a platform with the least amount since we might have ties
        foreach (var platform in platforms)
        {
            if (numPlayersOnPlatforms.TryGetValue(platform, out int playersOnThisPlatform) && playersOnThisPlatform == fewestAmountOfPlayers)
            {
                foreach (var client in Game.Clients)
                {
                    if (client.Pawn is GarrywarePlayer player && player.GroundEntity == platform)
                    {
                        player.FlagAsRoundWinner();
                    }
                }
            }
        }
    }

    public override void Cleanup()
    {
        numPlayersOnPlatforms.Clear();
        platforms.Clear();
    }
}
