using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class CrateColorRoulette : Microgame
{
    private BreakableProp crate;
    private TimeSince timeSinceColorChanged;
    private GameColor targetColor;
    private float changeColorTime = 1.0f;

    private readonly ShuffledDeck<float> rotationSpeedsDeck;
    
    public CrateColorRoulette()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.UseWeapon;
        GameLength = 10;
        
        rotationSpeedsDeck = new ShuffledDeck<float>();
        rotationSpeedsDeck.Add(1.0f, 1);
        rotationSpeedsDeck.Add(0.8f, 3);
        rotationSpeedsDeck.Add(0.6f, 3);
        rotationSpeedsDeck.Add(0.4f, 1);
    }
    
    public override void Setup()
    {
        // Choose a random rotation speed from the speeds deck to randomize the difficulty a bit
        changeColorTime = rotationSpeedsDeck.Next();
        
        // Spawn the roulette crate
        var spawn = CommonEntities.AboveBoxSpawnsDeck.Next();
        crate = new BreakableProp
        {
            Position = spawn.Position,
            Rotation = spawn.Rotation,
            Model = CommonEntities.Crate,
            CanGib = false,
            PhysicsEnabled = false,
            Indestructible = true,
            GameColor = GetRandomColor()
        };
        AutoCleanup(crate);
        crate.Damaged += OnCrateDamaged;

        timeSinceColorChanged = 0;
        
        ShowInstructions("Get ready...");
    }
    
    public override void Start()
    {
        // Pick a random target color to aim for and shuffle the deck
        targetColor = GetRandomColor();
        CommonEntities.ColorsDeck.Shuffle();
        
        GiveWeapon<Pistol>(To.Everyone);
        ShowInstructions($"Shoot {targetColor.AsName()}!");
    }

    public override void Tick()
    {
        base.Tick();

        // Change color every so often
        if (timeSinceColorChanged > changeColorTime)
        {
            crate.GameColor = GetRandomColor();
            timeSinceColorChanged = 0.0f;
        }
    }
    
    private void OnCrateDamaged(BreakableProp prop, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            if (prop.GameColor == targetColor)
            {
                player.FlagAsRoundWinner();
                player.RemoveWeapons();
            }
            else
            {
                player.FlagAsRoundLoser();
                player.RemoveWeapons();
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        crate = null;
    }
}