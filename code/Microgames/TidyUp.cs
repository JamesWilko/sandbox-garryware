using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class TidyUp : Microgame
{
    private static readonly Model BinModel = Model.Load("models/sbox_props/park_bin/park_bin.vmdl");

    private Dictionary<GarrywarePlayer, int> rubbishClearedPerPlayer = new();
    private int targetToClear;

    public TidyUp()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.Punt | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Empty, MicrogameRoom.Boxes };
        GameLength = 8;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        GiveWeapon<GravityGun>(To.Everyone);

        // Spawn some bins
        int numBins = (int) Math.Clamp(Room.OnFloorSpawns.Count * Rand.Float(0.15f, 0.4f), 1, 5);
        for (int i = 0; i < numBins; ++i)
        {
            var binProp = new BreakableProp
            {
                Transform = Room.OnFloorSpawnsDeck.Next().Transform,
                Model = BinModel,
                Indestructible = true,
                Static = true,
                PhysicsEnabled = false,
            };
            AutoCleanup(binProp);
            binProp.PhysicsCollision += OnPhysicsCollisionWithBin;
        }

        // Spawn a load of crap
        int numPlayers = Client.All.Count;
        int rubbishSpawned = (int) (numPlayers * Rand.Float(2.0f, 3.0f));
        for (int i = 0; i < rubbishSpawned; ++i)
        {
            var rubbish = new BreakableProp
            {
                Transform = Room.InAirSpawnsDeck.Next().Transform,
                Model = CommonEntities.RubbishDeck.Next(),
                Indestructible = true,
            };
            AutoCleanup(rubbish);
            rubbish.ApplyLocalImpulse(Vector3.Random * 1024.0f);
        }

        // Set how much rubbish we have to clean up
        targetToClear = numPlayers switch
        {
            <= 2 => 1,
            < 5 => 2,
            _ => 3
        };
        ShowInstructions(string.Format("Tidy up at least {0} pieces of rubbish!", targetToClear)); // @localization
    }

    private void OnPhysicsCollisionWithBin(CollisionEventData collisionData)
    {
        if(IsGameFinished())
            return;
        
        if (collisionData.Other.Entity is BreakableProp prop
            && prop.ClientLastPickedUpBy != null
            && prop.ClientLastPickedUpBy.IsValid()
            && prop.ClientLastPickedUpBy.Pawn is GarrywarePlayer player)
        {
            rubbishClearedPerPlayer[player] = rubbishClearedPerPlayer.GetValueOrDefault(player, 0) + 1;

            Particles.Create("particles/impact.smokepuff.vpcf", prop.Position).Destroy();
            Sound.FromEntity("sounds/balloon_pop_cute.sound", collisionData.This.Entity);
            prop.Delete();

            if (rubbishClearedPerPlayer[player] >= targetToClear && !player.HasLockedInResult)
            {
                player.FlagAsRoundWinner();
            }
        }
    }
    
    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        rubbishClearedPerPlayer.Clear();
    }
}