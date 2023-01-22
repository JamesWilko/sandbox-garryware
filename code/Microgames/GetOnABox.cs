using Sandbox;

namespace Garryware.Microgames;

public class GetOnABox : Microgame
{
    public GetOnABox()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 4;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.get-on-a-box");
        GiveWeapon<BallLauncher>(To.Everyone);
    }

    public override void Finish()
    {
        // Award win to players who are on boxes
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player && player.IsOnABox())
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
    }
}