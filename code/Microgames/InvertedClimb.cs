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
        ShowInstructions("Prepare yourself...");
    }

    public override void Start()
    {
        ShowInstructions("Climb onto a box!");
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.CameraMode = new InvertedFirstPersonCamera();
            }
        }
    }

    public override void Finish()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.CameraMode = new FirstPersonCamera();
                
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