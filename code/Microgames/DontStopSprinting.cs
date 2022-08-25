using Sandbox;

namespace Garryware.Microgames;

public class DontStopSprinting : Microgame
{
    public DontStopSprinting()
    {
        Rules = MicrogameRules.WinOnTimeout;
        ActionsUsedInGame = PlayerAction.Sprint;
    }
    
    public override void Setup()
    {
        ShowInstructions("Don't stop running!");
    }

    public override void Start()
    {
    }

    public override void Tick()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && !player.IsMovingAtSprintSpeed)
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
    }
}
