﻿using Sandbox;

namespace Garryware.Microgames;

public class DontGetHit : Microgame
{
    public DontGetHit()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Sprint;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        GameLength = 5;
        MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready-to-rumble");
    }

    public override void Start()
    {
        GiveWeapon<Fists>(To.Everyone);
        ShowInstructions("#microgame.instructions.hit-someone");

        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Hurt += OnPlayerHurt;
            }
        }
    }

    private void OnPlayerHurt(GarrywarePlayer victim, DamageInfo info)
    {
        if (!victim.HasLockedInResult)
        {
            victim.FlagAsRoundLoser();
            victim.RemoveWeapons();
        }

        if (info.Attacker is GarrywarePlayer attacker && !attacker.HasLockedInResult)
        {
            attacker.FlagAsRoundWinner();
            attacker.RemoveWeapons();
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Hurt -= OnPlayerHurt;
            }
        }
    }

    public override void Cleanup()
    {
    }
}