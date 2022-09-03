using System;
using System.Collections.Generic;
using System.Linq;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class FewestPlayersOnPlatform : Microgame
{
    private readonly Dictionary<Entity, int> numPlayersOnPlatforms = new();

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
        
        int maxPlatforms = Math.Max(Client.All.Count / 2, 1);
        int platforms = GetRandomAdjustedClientCount(0.3f, 0.6f, 1, maxPlatforms);
        for (int i = 0; i < platforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 5f,
                GameColor = CommonEntities.ColorsDeck.Next()
            };
            AutoCleanup(platform);
        }
    }

    public override void Finish()
    {
        // Count how many players on each platform
        foreach (var client in Client.All)
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
        
        // Get the platform with the fewest players
        Entity winningPlatform = numPlayersOnPlatforms.MinBy(kvp => kvp.Value).Key;
        
        // Award players on the platform the win
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && player.GroundEntity == winningPlatform)
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
        numPlayersOnPlatforms.Clear();
    }
}
