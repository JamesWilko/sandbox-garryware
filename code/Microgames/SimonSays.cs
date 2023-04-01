using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class SimonSays : Microgame
{
    // @note: this is very similar to the ShootInOrder game, might be able to reuse stuff between
    // them if we add another similar variant
    
    private readonly Dictionary<BreakableProp, int> propValues = new();
    private readonly Dictionary<GarrywarePlayer, int> playerIndices = new();
    private readonly List<BreakableProp> crates = new();

    private readonly List<string> soundEvents = new()
    {
        "microgame.chime.a",
        "microgame.chime.b",
        "microgame.chime.c",
        "microgame.chime.d",
        "microgame.chime.e",
        "microgame.chime.f",
        "microgame.chime.g",
    };
    
    private readonly ShuffledDeck<int> numberOfTargetsDeck = new();
    private readonly ShuffledDeck<float> speedDeck = new();
    
    private int numberOfTargets;
    private float beepTime;
    
    private const float BeforeBeepTime = 2.5f; // How long should we wait before beeping target so players can figure out whats going on 
    
    public SimonSays()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 8f;
    }
    
    protected virtual void BuildDifficultyDeck()
    {
        numberOfTargetsDeck.Clear();
        numberOfTargetsDeck.Add(4, 3);
        numberOfTargetsDeck.Add(5, 3);
        numberOfTargetsDeck.Add(6, 2);
        numberOfTargetsDeck.Add(7, 1);
        numberOfTargetsDeck.Shuffle();
        
        speedDeck.Clear();
        speedDeck.Add(1f, 3);
        speedDeck.Add(0.8f, 2);
        speedDeck.Add(0.7f, 2);
        speedDeck.Add(0.6f, 1);
        speedDeck.Add(0.5f, 1);
        speedDeck.Shuffle();
    }
    
    public override void Setup()
    {
        BuildDifficultyDeck();
        ShowInstructions("#microgame.instructions.pay-attention");

        numberOfTargets = numberOfTargetsDeck.Next();
        beepTime = speedDeck.Next();
        
        // Spawn the targets in
        propValues.Clear();
        for (int i = 0; i < numberOfTargets; ++i)
        {
            var crate = new BreakableProp
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false,
                Indestructible = true,
            };
            AutoCleanup(crate);
            crates.Add(crate);

            propValues.Add(crate, i); // Crates have an ascending value so we can keep track of which one the player hit easily
            crate.Damaged += OnCrateDamaged;
        }
        
        // Randomize the sound events list so the chimes are played in random order
        soundEvents.Shuffle();
        
        // Determine how long the warmup should be based on the number of targets
        WarmupLength = BeforeBeepTime + beepTime * numberOfTargets;
        BeepTargets();
    }
    
    private async void BeepTargets()
    {
        await TaskSource.DelaySeconds(BeforeBeepTime);

        for (int i = 0; i < numberOfTargets; ++i)
        {
            var crate = crates[i];
            
            crate.GameColor = (GameColor)(i + 1);
            Sound.FromEntity(soundEvents[i], crate);
            
            await TaskSource.DelaySeconds(beepTime);
            crate.GameColor = GameColor.White;
        }
    }

    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
        ShowInstructions("#microgame.instructions.repeat-pattern");
    }

    private void OnCrateDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player
            && !player.HasLockedInResult)
        {
            // Make sure the player has a result
            if (!playerIndices.ContainsKey(player))
            {
                playerIndices.Add(player, -1);
            }
            
            // Check if what the player hit is the next in their sequence
            int nextIndex = playerIndices[player] + 1;
            int hitValue = propValues[crate];
            
            if (hitValue == nextIndex)
            {
                playerIndices[player] = nextIndex;
                Sound.FromEntity(To.Single(attacker.Client), soundEvents[hitValue], crate);
                
                // Check if we finished the sequence
                if (nextIndex == numberOfTargets - 1)
                {
                    player.FlagAsRoundWinner();
                }
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
        playerIndices.Clear();
        numberOfTargetsDeck.Shuffle();
        crates.Clear();
    }
    
}
