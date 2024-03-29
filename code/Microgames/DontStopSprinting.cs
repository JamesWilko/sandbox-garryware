﻿using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players have to keep sprinting, if they slow down from their sprinting speed they lose.
/// </summary>
public class DontStopSprinting : Microgame
{
    public DontStopSprinting()
    {
        Rules = MicrogameRules.WinOnTimeout;
        ActionsUsedInGame = PlayerAction.Sprint;
        ShowActionsToPlayer = ShowGameActions.AfterSetup;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        WarmupLength = 2;
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.dont-stop-running");
    }

    public override void Start()
    {
    }

    public override void Tick()
    {
        foreach (var client in Game.Clients)
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
