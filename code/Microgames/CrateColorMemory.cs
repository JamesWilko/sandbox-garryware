using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A bunch of crates spawn in with different colors. After a short delay the colors vanish and players have to shoot the crate that was the color instructed.
/// </summary>
public class CrateColorMemory : Microgame
{
    private readonly List<BreakableProp> crates = new();
    private GameColor targetColor;

    public CrateColorMemory()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.UseWeapon;
        GameLength = 3;
    }
    
    public override void Setup()
    {
        int cratesSpawned = Random.Shared.Int(4, 6);
        for (int i = 0; i < cratesSpawned; ++i)
        {
            var spawn = CommonEntities.AboveBoxSpawnsDeck.Next();
            var ent = new BreakableProp
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                Model = CommonEntities.Crate,
                CanGib = false,
                PhysicsEnabled = false,
                Indestructible = true,
                GameColor = GetRandomColor()
            };
            crates.Add(ent);
            AutoCleanup(ent);
            
            ent.Damaged += OnCrateDamaged;
        }
        
        ShowInstructions("Get ready...");
    }
    
    public override void Start()
    {
        targetColor = GetRandomColorAlreadyInUse();
        GiveWeapon<GWPistol>(To.Everyone);
        
        foreach (var crate in crates)
        {
            crate.HideGameColor = true;
        }
        
        ShowInstructions($"Shoot {targetColor.AsName()}!");
    }
    
    private void OnCrateDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            if (crate.GameColor == targetColor)
            {
                player.FlagAsRoundWinner();
            }
            else
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        crates.Clear();
    }
    
}