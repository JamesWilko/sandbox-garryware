using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class DontFallOffPlatform : Microgame
{
    private Particles particles;
    private Vector3 centerPoint;
    private float centerSize;

    public DontFallOffPlatform()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
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
        
        ShowInstructions("#microgame.instructions.dont-fall-off.phase-1");
        
        particles = Particles.Create("particles/microgame.platforms.red.vpcf", centerPoint);
        particles.SetPosition(1, new Vector3(centerSize, 0, 0));
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.dont-fall-off.phase-2");
        GiveWeapon<BallLauncher>(To.Everyone);
    }

    public override void Tick()
    {
        base.Tick();
        
        // Check if somebody fell off the edge
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && !player.HasLockedInResult && player.Position.z < -24.0f)
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    public override void Finish()
    {
        // Check if people are still in the center and fail them if they are
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && !player.HasLockedInResult)
            {
                var distanceToCenter = Vector3.DistanceBetween(centerPoint, player.Position);
                if (distanceToCenter < centerSize)
                {
                    player.FlagAsRoundLoser();
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
            RoomSize.Small => 208,
            RoomSize.Medium => 0,
            RoomSize.Large => 456,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}