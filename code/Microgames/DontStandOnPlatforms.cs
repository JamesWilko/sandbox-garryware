using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class DontStandOnPlatforms : Microgame
{
    public DontStandOnPlatforms()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Empty };
        WarmupLength = 2;
        GameLength = 7;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.dont-stand-on-platform");

        int numPlatforms = (int)(Room.OnFloorSpawns.Count * Rand.Float(0.7f, 0.8f) + 0.5f);
        for (int i = 0; i < numPlatforms; ++i)
        {
            var platform = new BiggerPlatform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 2f,
                GameColor = GetRandomColor()
            };
            AutoCleanup(platform);
        }
    }

    public override void Start()
    {
        GiveWeapon<BallLauncher>(To.Everyone);
    }

    public override void Tick()
    {
        base.Tick();
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player
                && !player.HasLockedInResult
                && player.GroundEntity is Platform)
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}