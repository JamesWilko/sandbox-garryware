using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class Blackjack : Microgame
{
    private readonly Dictionary<BreakableProp, int> propValues = new();
    private readonly Dictionary<GarrywarePlayer, int> playerValues = new();

    private readonly ShuffledDeck<int> cardCountDeck = new();
    private readonly ShuffledDeck<int> decoyValuesDeck = new();

    public Blackjack()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 7;
        
        cardCountDeck.Add(3, 2);
        cardCountDeck.Add(4, 2);
        cardCountDeck.Add(5);
        cardCountDeck.Shuffle();
        
        decoyValuesDeck.Add(2, 3);
        decoyValuesDeck.Add(4, 3);
        decoyValuesDeck.Add(5, 3);
        decoyValuesDeck.Add(6, 3);
        decoyValuesDeck.Add(8, 3);
        decoyValuesDeck.Add(9, 3);
        decoyValuesDeck.Add(10);
        decoyValuesDeck.Add(11);
        decoyValuesDeck.Shuffle();
    }

    private BlackjackCards.CardSet[] GetRandomCardSet()
    {
        return cardCountDeck.Next() switch
        {
            3 => BlackjackCards.ThreeCardSets,
            4 => BlackjackCards.FourCardSets,
            5 => BlackjackCards.FiveCardSets,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");

        const int numCrates = 5;
        
        // Determine which set we should pull from
        var cardSet = GetRandomCardSet();
        var targetCards = Rand.FromArray(cardSet);

        // Create a target for each card
        for (int i = 0; i < targetCards.CardValues.Length; ++i)
        {
            var crate = new BreakableProp
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false,
                Indestructible = true,
                ShowWorldText = true,
                WorldText = targetCards.CardValues[i].ToString("N0")
            };
            AutoCleanup(crate);
            crate.Damaged += OnCrateDamaged;
            
            propValues.Add(crate, targetCards.CardValues[i]);
        }
        
        // Fill in the remaining space with dummies which might have a use or not
        for (int i = targetCards.CardValues.Length + 1; i <= numCrates; ++i)
        {
            int decoyValue = decoyValuesDeck.Next();
            var crate = new BreakableProp
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false,
                Indestructible = true,
                ShowWorldText = true,
                WorldText = decoyValue.ToString("N0")
            };
            AutoCleanup(crate);
            crate.Damaged += OnCrateDamaged;
            
            propValues.Add(crate, decoyValue);
        }
        
    }

    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
        ShowInstructions("#microgame.instructions.blackjack");
    }
    
    private void OnCrateDamaged(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player
            && !player.HasLockedInResult
            && propValues.TryGetValue(crate, out var crateValue))
        {
            // Make sure player exists in the values lookup
            if (!playerValues.ContainsKey(player))
            {
                playerValues.Add(player, 0);
            }
            
            // Increase player value by what they hit
            playerValues[player] += crateValue;

            // Let them know the hit the crate
            SoundUtility.PlayTargetHit(To.Single(player));

            // Check if the player has gone bust
            if (playerValues[player] > 21)
            {
                player.FlagAsRoundLoser();
                GameEvents.NewInstructions(To.Single(player), "#microgame.instructions.blackjack.bust", 2.0f); // @localization
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();

        foreach (var pair in playerValues)
        {
            if (pair.Value == 21)
            {
                pair.Key.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
        propValues.Clear();
        playerValues.Clear();
    }
}
