using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class StayInCenterPlatform : Microgame
{
    private Particles particles;
    private Vector3 centerPoint;
    private float centerSize;

    public StayInCenterPlatform()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Platform };
        WarmupLength = 3;
        GameLength = 6;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        centerPoint = Room.OnFloorSpawnsDeck.Next().Position;
        centerSize = GetCenterSizeForRoom();
        
        ShowInstructions("#microgame.instructions.get-to-center-platform");
        
        particles = Particles.Create("particles/microgame.platforms.green.vpcf", centerPoint);
        particles.SetPosition(1, new Vector3(centerSize, 0, 0));
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.dont-fall-off.phase-2");
        GiveWeapon<RocketLauncher>(To.Everyone);
    }
    
    public override void Finish()
    {
        // Check if people are still in the center and make them win if they are
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && !player.HasLockedInResult)
            {
                var distanceToCenter = Vector3.DistanceBetween(centerPoint.WithZ(player.Position.z), player.Position);
                if (distanceToCenter < centerSize)
                {
                    player.FlagAsRoundWinner();
                }
            }
        }
        
        // Stop the particles
        particles.Destroy();
    }

    public override void Cleanup()
    {
        particles = null;
    }

    private float GetCenterSizeForRoom()
    {
        return Room.Size switch
        {
            RoomSize.Small => 64,
            RoomSize.Medium => 80,
            RoomSize.Large => 108,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}