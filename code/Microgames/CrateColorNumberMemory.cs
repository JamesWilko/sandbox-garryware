using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A bunch of crates spawn in with different colors and numbers. After a short delay the colors and numbers vanish and players have to shoot the correct answer.
/// </summary>
public class CrateColorNumberMemory : Microgame
{
    private readonly List<BreakableProp> crates = new();
    private readonly ShuffledDeck<int> randomValues = new();

    private bool targetingColor;
    private GameColor targetColor;
    private string targetValue;

    public CrateColorNumberMemory()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 3;
        
        for(int i = 10; i < 100; i++)
            randomValues.Add(i);
        randomValues.Shuffle();
    }
    
    public override void Setup()
    {
        int cratesSpawned = Random.Shared.Int(3, 4);
        for (int i = 0; i < cratesSpawned; ++i)
        {
            var spawn = Room.AboveBoxSpawnsDeck.Next();
            var ent = new BreakableProp
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                Model = CommonEntities.Crate,
                CanGib = false,
                PhysicsEnabled = false,
                Indestructible = true,
                GameColor = GetRandomColor(),
                ShowWorldText = true,
                WorldText = randomValues.Next().ToString("N0")
            };
            crates.Add(ent);
            AutoCleanup(ent);
            
            ent.Damaged += OnCrateDamaged;
        }
        
        ShowInstructions("#microgame.look-carefully");
    }
    
    public override void Start()
    {
        targetingColor = Rand.Float() > 0.5f;
        targetColor = GetRandomColorAlreadyInUse();
        targetValue = randomValues.GetRandomUsedElement().ToString("N0");
        
        GiveWeapon<Pistol>(To.Everyone);
        
        foreach (var crate in crates)
        {
            crate.HideGameColor = true;
            crate.ShowWorldText = false;
        }

        var target = targetingColor ? targetColor.AsName() : targetValue;
        ShowInstructions($"Shoot {target}!"); // @localization
    }
    
    private void OnCrateDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            if ((targetingColor && crate.GameColor == targetColor)
                || (!targetingColor && crate.WorldText == targetValue))
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
        randomValues.Shuffle();
    }
    
}