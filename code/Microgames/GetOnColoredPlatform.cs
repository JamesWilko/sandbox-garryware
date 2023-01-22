using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class GetOnColoredPlatform : Microgame
{
    private GameColor correctColor;

    public GetOnColoredPlatform()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Empty };
        GameLength = 5;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        int fakePlatforms = NumberOfCorrectPlatforms * 3;
        correctColor = GetRandomColor();
        
        // Create the correct platforms
        for (int i = 0; i < NumberOfCorrectPlatforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 5f,
                GameColor = correctColor
            };
            AutoCleanup(platform);
        }

        // Create the decoy platforms
        for (int i = 0; i < fakePlatforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 5f,
                GameColor = GetRandomColorExcept(correctColor)
            };
            AutoCleanup(platform);
        }

        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        GiveWeapon<BallLauncher>(To.Everyone);
        ShowInstructions(string.Format("Stand on the {0} platform!", correctColor.AsName())); // @localization
    }

    public override void Finish()
    {
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player
                && player.GroundEntity != null
                && player.GroundEntity is Platform platform
                && platform.GameColor == correctColor)
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
    }

    private int NumberOfCorrectPlatforms => Room.Size switch
    {
        RoomSize.Small => 1,
        RoomSize.Medium => 2,
        RoomSize.Large => 3,
        _ => throw new ArgumentOutOfRangeException()
    };

}