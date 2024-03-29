﻿using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class ShootTargetExactNumber : Microgame
{
    private int targetHits;
    private readonly Dictionary<GarrywarePlayer, int> playerHits = new();
    private ShuffledDeck<float> scalesDeck;
    
    public ShootTargetExactNumber()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.DontShowEndOfRoundStats;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Empty, MicrogameRoom.Boxes };
        GameLength = 10;

        scalesDeck = new ShuffledDeck<float>();
        scalesDeck.Add(5.0f, 3);
        scalesDeck.Add(3.0f, 3);
        scalesDeck.Add(2.0f, 1);
    }
    
    public override void Setup()
    {
        // Determine how many times we need to hit the target
        targetHits = Game.Random.Int(5, 18);
        
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
        ShowInstructions(string.Format("Shoot the target exactly {0} times!", targetHits)); // @localization
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
            SoundUtility.PlayTargetHit(To.Single(player));
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        
        // Assign everybody who shot it the correct amount a win
        foreach (var playerPair in playerHits)
        {
            if (playerPair.Value == targetHits)
            {
                playerPair.Key.FlagAsRoundWinner();
            }
            else
            {
                GameEvents.SendIntegerStat(To.Single(playerPair.Key), RoundStat.YouHitTheTargetXTimes_Failed, playerPair.Value);
            }
        }
    }

    public override void Cleanup()
    {
        playerHits.Clear();
    }
}
