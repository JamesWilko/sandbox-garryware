using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players must break a crate to win. They can only break one and there aren't enough crates for all players.
/// </summary>
public class BreakCrates : Microgame
{
    private int cratesSpawned;
    
    public BreakCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.break-a-crate");
    }

    public override void Start()
    {
        GiveWeapon<Fists>(To.Everyone);
        
        cratesSpawned = Math.Clamp((int) Math.Ceiling(Client.All.Count * Random.Shared.Float(0.5f, 0.75f)), 1, Client.All.Count);
        for (int i = 0; i < cratesSpawned; ++i)
        {
            var spawn = Room.OnBoxSpawnsDeck.Next();
            var ent = new BreakableProp
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                Model = CommonEntities.Crate,
            };
            AutoCleanup(ent);
            
            ent.OnBroken += OnCrateDestroyed;
        }
    }
    
    private void OnCrateDestroyed(BreakableProp crate, Entity attacker)
    {
        if(IsGameFinished())
            return;
        
        if (attacker is GarrywarePlayer player)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
        
        cratesSpawned--;
        if (cratesSpawned == 0)
            EarlyFinish();
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}
