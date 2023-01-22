using Sandbox;

namespace Garryware.Microgames;

public class StayGrounded : Microgame
{
    public StayGrounded()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        WarmupLength = 2;
        GameLength = 5;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.stay-grounded.start");
    }

    public override void Start()
    {
        GiveWeapon<LauncherPistol>(To.Everyone);
        ShowInstructions("#microgame.instructions.stay-grounded.dont-get-hit");
    }

    public override void Tick()
    {
        base.Tick();
        
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player && player.GroundEntity == null)
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