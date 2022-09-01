using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players must break all the crates in the map to win. Everybody who broke a crate will win, but only if all the crates are broken.
/// </summary>
public class BreakAllCrates : Microgame
{
    private int cratesRemaining;
    private List<GarrywarePlayer> potentialWinners = new();
    
    public BreakAllCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 7;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.break-every-crate");
    }

    public override void Start()
    {
        GiveWeapon<Fists>(To.Everyone);

        cratesRemaining = GetRandomAdjustedClientCount(1.25f, 2.0f, Client.All.Count, Room.OnBoxSpawns.Count);
        for (int i = 0; i < cratesRemaining; ++i)
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
        
        if (attacker is GarrywarePlayer player && !potentialWinners.Contains(player))
        {
            potentialWinners.Add(player);
        }
        
        cratesRemaining--;
        if (cratesRemaining == 0)
        {
            foreach (var winnerPlayer in potentialWinners)
            {
                winnerPlayer.FlagAsRoundWinner();
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        potentialWinners.Clear();
    }
}