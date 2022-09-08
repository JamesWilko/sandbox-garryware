using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class ShootInOrder : Microgame
{
    private bool ascending;
    private readonly List<int> targetValues = new();
    private readonly Dictionary<BreakableProp, int> propValues = new();
    private readonly Dictionary<GarrywarePlayer, int> playerIndices = new();

    private readonly ShuffledDeck<int> difficultyDeck = new();

    public ShootInOrder()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn | MicrogameRules.DontClearInstructions;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        WarmupLength = 3f;
        GameLength = 8f;
        
        difficultyDeck.Add(4, 3);
        difficultyDeck.Add(5, 3);
        difficultyDeck.Add(6, 2);
        difficultyDeck.Add(7, 1);
        difficultyDeck.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
        
        // Determine what values each target should have
        int numTargets = difficultyDeck.Next();
        targetValues.Clear();
        for (int i = 0; i < numTargets; ++i)
        {
            int value = Rand.Int(10, 999);
            while (targetValues.Contains(value))
            {
                value = Rand.Int(10, 999);
            }
            targetValues.Add(value);
        }
        
        // Determine what order we need to shoot them in
        ascending = Rand.Float() >= 0.5f;
        if (ascending)
        {
            targetValues.Sort();
        }
        else
        {
            targetValues.Sort((a, b) => a.CompareTo(b) * -1);
        }
        
        // Spawn the targets in
        propValues.Clear();
        for (int i = 0; i < targetValues.Count; ++i)
        {
            var crate = new BreakableProp
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false,
                Indestructible = true,
                ShowWorldText = true,
                WorldText = targetValues[i].ToString("N0")
            };
            AutoCleanup(crate);

            propValues.Add(crate, targetValues[i]);
            crate.Damaged += OnCrateDamaged;
        }
    }

    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
        ShowInstructions(ascending ? "#microgame.instructions.shoot-in-order.ascending" : "#microgame.instructions.shoot-in-order.descending");
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
            int nextValue = targetValues[nextIndex];
            int hitValue = propValues[crate];

            if (hitValue == nextValue)
            {
                playerIndices[player] = nextIndex;
                
                // Check if we finished the sequence
                if (nextIndex == targetValues.Count - 1)
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
        ShowInstructions($"The correct order was {string.Join(", ", targetValues)}!"); // @localization
    }

    public override void Cleanup()
    {
        playerIndices.Clear();
        difficultyDeck.Shuffle();
    }
}
