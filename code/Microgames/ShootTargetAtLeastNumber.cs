using System.Collections.Generic;
using System.Linq;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class ShootTargetAtLeastNumber : Microgame
{
    private int targetHits;
    private readonly Dictionary<GarrywarePlayer, int> playerHits = new();
    private ShuffledDeck<float> scalesDeck;

    private const float statDisplayTime = 2.0f;
    
    public ShootTargetAtLeastNumber()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.DontClearInstructions;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Empty, MicrogameRoom.Boxes };
        GameLength = 8;

        scalesDeck = new ShuffledDeck<float>();
        scalesDeck.Add(5.0f, 3);
        scalesDeck.Add(3.0f, 3);
        scalesDeck.Add(2.0f, 1);
    }
    
    public override void Setup()
    {
        // Determine how many times we need to hit the target
        targetHits = Rand.Int(5, 20);
        
        // Spawn the targets to shoot at
        int targetsToSpawn = Room.Size == RoomSize.Large ? 2 : 1;
        for (int i = 0; i < targetsToSpawn; ++i)
        {
            var targetEntity = new FloatingTarget
            {
                Transform = Room.InAirSpawnsDeck.Next().Transform
            };
            AutoCleanup(targetEntity);
            targetEntity.Scale = scalesDeck.Next();
            targetEntity.Damaged += OnTargetDamaged;
        }

        // Send instructions to the players
        ShowInstructions(string.Format("Shoot the target at least {0} times!", targetHits)); // @localization
    }
    
    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
    }
    
    private void OnTargetDamaged(BreakableProp self, Entity attacker)
    {
        if (attacker is GarrywarePlayer player)
        {
            playerHits[player] = playerHits.GetValueOrDefault(player) + 1;
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        
        // Assign everybody who shot it at least the correct amount a win
        foreach (var playerPair in playerHits)
        {
            if (playerPair.Value >= targetHits)
            {
                playerPair.Key.FlagAsRoundWinner();
                GameEvents.NewInstructions(To.Single(playerPair.Key), string.Format("You hit it {0} times!", playerPair.Value), statDisplayTime); // @localization
            }
            else
            {
                GameEvents.NewInstructions(To.Single(playerPair.Key), string.Format("You only hit it {0} times!", playerPair.Value), statDisplayTime); // @localization
            }
        }
    }

    public override void Cleanup()
    {
        playerHits.Clear();
    }
}
