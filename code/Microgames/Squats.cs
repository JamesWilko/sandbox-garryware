using System.Collections.Generic;
using Sandbox;

namespace Garryware.Microgames;

public class Squats : Microgame
{
    private int target;
    private readonly Dictionary<GarrywarePlayer, int> numPlayerSquats = new();

    public Squats()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.Crouch;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        GameLength = 10;
        UiClass = "MicrogameUiSquats";
    }
    
    public override void Setup()
    {
        target = Game.Random.Int(4, 10);
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions(string.Format("Squat at least {0} times!", target)); // @localization

        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Squatted += OnPlayerSquatted;
            }
        }
    }

    private void OnPlayerSquatted(GarrywarePlayer player)
    {
        numPlayerSquats[player] = numPlayerSquats.GetValueOrDefault(player, 0) + 1;
        if (numPlayerSquats[player] >= target)
        {
            player.FlagAsRoundWinner();
        }
        else
        {
            SoundUtility.PlayTargetHit(To.Single(player));
        }
    }

    public override void Finish()
    {
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Squatted -= OnPlayerSquatted;
            }
        }
    }

    public override void Cleanup()
    {
        numPlayerSquats.Clear();
    }
    
}
