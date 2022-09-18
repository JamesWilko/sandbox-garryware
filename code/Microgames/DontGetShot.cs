using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class DontGetShot : Microgame
{
    private readonly List<TurretNpc> turrets = new();
    
    public DontGetShot()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Crouch | PlayerAction.Sprint | PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        WarmupLength = 3;
        GameLength = 10;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.dont-get-shot");

        int numTurrets = Client.All.Count switch
        {
            < 3 => 1,
            < 5 => 2,
            < 9 => 3,
            < 12 => 4,
            _ => 5
        };
        
        for (int i = 0; i < numTurrets; ++i)
        {
            var turret = new TurretNpc()
            {
                Position = Room.OnBoxSpawnsDeck.Next().Position,
            };
            AutoCleanup(turret);
            turrets.Add(turret);
        }
    }

    public override void Start()
    {
        GiveWeapon<BallLauncher>(To.Everyone);

        // Check if a player got shot
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Hurt += PlayerOnHurt;
            }
        }
        
        // Let the turrets shoot
        foreach (var npc in turrets)
        {
            npc.CanFire = true;
        }
    }

    private void PlayerOnHurt(GarrywarePlayer victim, DamageInfo info)
    {
        if (!victim.HasLockedInResult && info.Flags.HasFlag(DamageFlags.Bullet))
        {
            victim.FlagAsRoundLoser();
            
            if (info.Attacker is TurretNpc npc)
            {
                npc.OnEliminatedPlayer();
            }
        }
    }

    public override void Finish()
    {
        // No more shooting
        foreach (var npc in turrets)
        {
            npc.CanFire = false;
        }
        
        // Un-listen to the events so we don't accidentally ruin another gamemode
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.Hurt -= PlayerOnHurt;
            }
        }
    }

    public override void Cleanup()
    {
        turrets.Clear();
    }
    
}