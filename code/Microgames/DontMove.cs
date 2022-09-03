using Sandbox;

namespace Garryware.Microgames;

public class DontMove : Microgame
{
    public DontMove()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        WarmupLength = 1.5f;
        GameLength = 5;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.dont-move");
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
                && player.Velocity.LengthSquared > 10f)
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    public override void Finish()
    {
    }

    public override void Cleanup()
    {
        RemoveAllWeapons();
    }
    
}
