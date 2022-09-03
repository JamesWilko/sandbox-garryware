using Garryware.Entities;
using Sandbox;
using System.Collections.Generic;

namespace Garryware.Microgames;

public class BoxJump : Microgame
{
    class JumpInfo
    {
        public int JumpCount = 0;
        public OnBoxTrigger LastBoxTrigger = null;
    }

    private readonly Dictionary<GarrywarePlayer, JumpInfo> playerJumps = new();
    private int targetJumps;

    public BoxJump()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Sprint | PlayerAction.Jump;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 7;
    }

    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        // TODO: Randomise this number maybe?
        targetJumps = 3;
        ShowInstructions(string.Format("Jump between boxes at least {0} times!", targetJumps)); // @localization
    }

    public override void Tick()
    {
        base.Tick();

        foreach (var client in Client.All)
        {
            if (!(client.Pawn is GarrywarePlayer player) || player.HasLockedInResult)
            {
                continue;
            }

            // Make sure player exists in the values lookup
            if (!playerJumps.ContainsKey(player))
            {
                playerJumps.Add(player, new JumpInfo());
            }

            // If they're on the floor, reset their jump count
            var jumpInfo = playerJumps[player];
            if (player.IsOnTheFloor())
            {
                jumpInfo.JumpCount = 0;
                jumpInfo.LastBoxTrigger = null;
                continue;
            }

            // Get the block they're on
            var blockTrigger = player.GetOnBoxTrigger();
            if (blockTrigger == null)
            {
                continue;
            }

            // If they're on a new box, increase their jump count
            if (blockTrigger != jumpInfo.LastBoxTrigger)
            {
                jumpInfo.JumpCount += 1;
                jumpInfo.LastBoxTrigger = blockTrigger;

                if (jumpInfo.JumpCount > targetJumps)
                {
                    player.FlagAsRoundWinner();
                }
            }
        }
    }

    public override void Cleanup()
    {
        playerJumps.Clear();
    }

    public override void Finish()
    {
    }
}
