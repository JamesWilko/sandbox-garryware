using Sandbox;

namespace Garryware.Microgames;

public class InvertedClimb : Microgame
{
    public InvertedClimb()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.Jump;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.inverted-climb");
        
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.OverrideCameraMode(CameraMode.FirstPersonInverted);
            }
        }
    }

    public override void Finish()
    {
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.RestoreNormalCamera();
                
                if (player.IsOnABox())
                {
                    player.FlagAsRoundWinner();
                }
            }
        }
    }

    public override void Cleanup()
    {
    }
}