using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class StandOnMissingProp : Microgame
{
    private readonly ShuffledDeck<Entity> crates = new();
    private readonly List<OnBoxTrigger> winningTriggers = new();
    
    private int propsToSpawn;
    private int propsToRemove;
    
    public StandOnMissingProp()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        WarmupLength = 3;
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.look-carefully");
        
        propsToSpawn = (int) Math.Ceiling(Room.OnBoxSpawns.Count * Rand.Float(0.5f, 0.7f));
        propsToRemove = GetRandomAdjustedClientCount(0.25f, 0.5f, 1, (int)(propsToSpawn * 0.35f));
        
        // Spawn the props
        for (int i = 0; i < propsToSpawn; ++i)
        {
            var spawn = Room.OnBoxSpawnsDeck.Next();
            var crate = new BreakableProp()
            {
                Position = spawn.Position,
                Rotation = new Angles(0f, Rand.Float(0f, 360f), 0f).ToRotation(),
                Model = CommonEntities.Crate,
                Indestructible = true,
                PhysicsEnabled = false
            };
            AutoCleanup(crate);
            crates.Add(crate);
        }
        crates.Shuffle();
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.stand-on-missing");
        GiveWeapon<BallLauncher>(To.Everyone);
        
        // Remove a bunch of the props, save the trigger that they are a part of
        for (int i = 0; i < propsToRemove; ++i)
        {
            var toRemove = crates.Next();
            foreach (var trigger in Room.OnBoxTriggers)
            {
                if (trigger.ContainsEntity(toRemove))
                {
                    winningTriggers.Add(trigger);
                    break;
                }
            }
            toRemove.Delete();
        }
    }

    public override void Finish()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player
                && winningTriggers.Contains(player.GetOnBoxTrigger()))
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
        crates.Clear();
        winningTriggers.Clear();
    }
    
}