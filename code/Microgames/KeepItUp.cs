using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A number of props spawn that the players have to shoot to fling them into the air. If a prop touches the ground, it resets the list of players who shot it.
/// Players have to have shot a prop that was in the air when the time runs out.
/// </summary>
public class KeepItUp : Microgame
{
    private static readonly Model BinModel = Model.Load("models/citizen_props/trashcan01.vmdl");
    private readonly Dictionary<GarrywarePlayer, BreakableProp> lastPropPlayersShot = new();

    public KeepItUp()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 6f;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
        GiveWeapon<Pistol>(To.Everyone);
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.keep-it-up");

        int numBins = GetRandomAdjustedClientCount(0.6f, 0.8f);
        for (int i = 0; i < numBins; ++i)
        {
            var spawnLocation = Room.Contents == MicrogameRoom.Boxes ? Room.OnBoxSpawnsDeck.Next().Position : Room.OnFloorSpawnsDeck.Next().Position;
            var bin = new BreakableProp()
            {
                Position = spawnLocation,
                Model = BinModel,
                Indestructible = true
            };
            AutoCleanup(bin);

            bin.Damaged += BinOnDamaged;
            bin.PhysicsCollision += BinOnPhysicsCollision;
        }
    }
    
    private void BinOnDamaged(BreakableProp bin, Entity attacker)
    {
        const float maxAngle = 15f;
        
        var x = (float)Math.Sin(Game.Random.Float(-maxAngle, maxAngle).DegreeToRadian());
        var y = (float)Math.Sin(Game.Random.Float(-maxAngle, maxAngle).DegreeToRadian());
        var direction = new Vector3(x, y, 1f);
        bin.ApplyAbsoluteImpulse(direction * Game.Random.Float(400f, 600f));
        bin.ApplyLocalAngularImpulse(Vector3.Random.Normal * Game.Random.Float(300f, 600f));

        if (attacker is GarrywarePlayer player)
        {
            if (!lastPropPlayersShot.ContainsKey(player))
            {
                lastPropPlayersShot.Add(player, bin);
            }
            lastPropPlayersShot[player] = bin;
        }
    }
    
    private void BinOnPhysicsCollision(CollisionEventData collisionData)
    {
        // Check if the prop hit the ground
        if (collisionData.Other.Entity.IsWorld && collisionData.Normal.z < -0.98f)
        {
            // If it did then clear what players last hit it
            foreach (var pair in lastPropPlayersShot)
            {
                if (pair.Value == collisionData.This.Entity)
                {
                    lastPropPlayersShot[pair.Key] = null;
                }
            }
        }
    }
    
    public override void Finish()
    {
        // Any player who shot a bin that didn't touch the ground is a winner
        foreach (var pair in lastPropPlayersShot)
        {
            if (pair.Value != null)
            {
                pair.Key.FlagAsRoundWinner();
            }
        }
    }

    public override void Cleanup()
    {
        lastPropPlayersShot.Clear();
    }
}