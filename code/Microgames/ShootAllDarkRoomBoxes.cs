using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class ShootAllDarkRoomBoxes : Microgame
{
    private ShuffledDeck<int> difficultyDeck;
    private int numBoxes;
    private Dictionary<GarrywarePlayer, HashSet<Entity>> boxesHitPerPlayer = new();

    public ShootAllDarkRoomBoxes()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.DarkRoom };
        GameLength = 7;

        difficultyDeck = new ShuffledDeck<int>();
        difficultyDeck.Add(4, 1);
        difficultyDeck.Add(5, 3);
        difficultyDeck.Add(6, 3);
        difficultyDeck.Add(7, 2);
        difficultyDeck.Add(8, 1);
        difficultyDeck.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.dark-room-shoot-all-boxes");

        numBoxes = difficultyDeck.Next();
        for (int i = 0; i < numBoxes; ++i)
        {
            var crate = new BreakableProp()
            {
                Position = Room.AboveBoxSpawnsDeck.Next().Position,
                PhysicsEnabled = false,
                Model = CommonEntities.Crate,
                Indestructible = true
            };
            AutoCleanup(crate);
            crate.Damaged += OnDamagedCrate;
        }
    }
    
    public override void Start()
    {
        GiveWeapon<FlashlightPistol>(To.Everyone);
    }

    private void OnDamagedCrate(BreakableProp boxHit, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            // Use a hashset per player of what boxes they've hit so they have to hit them all, instead of only hitting the same box over and over
            boxesHitPerPlayer.EnsureKeyExists(player, new HashSet<Entity>());
            if (boxesHitPerPlayer[player].Add(boxHit))
            {
                SoundUtility.PlayTargetHit(To.Single(player));
            }

            if (boxesHitPerPlayer[player].Count >= numBoxes)
            {
                player.FlagAsRoundWinner();
            }
        }
    }
    
    public override void Finish()
    {
    }

    public override void Cleanup()
    {
        RemoveAllWeapons();

        // Just clear what the player hit, we might play this mode twice so lets keep the hash set but empty it out
        foreach (var pair in boxesHitPerPlayer)
        {
            pair.Value.Clear();
        }
    }
    
}