using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class TidyUpEverything : Microgame
{
    private static readonly Model BinModel = Model.Load("models/sbox_props/park_bin/park_bin.vmdl");
    private static readonly ShuffledDeck<Model> RubbishModels = new();

    private List<GarrywarePlayer> potentialWinners = new();
    private int rubbishRemaining;
    
    public TidyUpEverything()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Empty, MicrogameRoom.Boxes };
        GameLength = 8;
        
        RubbishModels.Add(Model.Load("models/citizen_props/bathroomsink01.vmdl"), 2);
        RubbishModels.Add(Model.Load("models/sbox_props/pizza_box/pizza_box.vmdl"), 5);
        RubbishModels.Add(Model.Load("models/sbox_props/bin/rubbish_bag.vmdl"), 3);
        RubbishModels.Add(Model.Load("models/sbox_props/burger_box/burger_box.vmdl"), 3);
        RubbishModels.Add(Model.Load("models/citizen_props/trashbag02.vmdl"), 5);
        RubbishModels.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.tidy-up");
        GiveWeapon<GravityGun>(To.Everyone);

        // Spawn some bins
        int numBins = (int) Math.Clamp(Room.OnFloorSpawns.Count * Rand.Float(0.25f, 0.5f), 1, 5);
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
        rubbishRemaining = (int) (Client.All.Count * Rand.Float(2.0f, 3.0f));
        for (int i = 0; i < rubbishRemaining; ++i)
        {
            var rubbish = new BreakableProp
            {
                Transform = Room.InAirSpawnsDeck.Next().Transform,
                Model = RubbishModels.Next(),
                Indestructible = true,
            };
            AutoCleanup(rubbish);
            rubbish.ApplyLocalImpulse(Vector3.Random * 1024.0f);
        }
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
            potentialWinners.AddUnique(player);

            Particles.Create("particles/impact.smokepuff.vpcf", prop.Position).Destroy();
            Sound.FromEntity("sounds/balloon_pop_cute.sound", collisionData.This.Entity);
            prop.Delete();

            rubbishRemaining--;
            AttemptLockInWinners();
        }
    }

    private void AttemptLockInWinners()
    {
        // Can only win if there is no rubbish remaining
        if (rubbishRemaining > 0)
            return;
        
        foreach (var player in potentialWinners)
        {
            if(!player.HasLockedInResult)
                player.FlagAsRoundWinner();
        }
    }
    
    public override void Finish()
    {
        AttemptLockInWinners();
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        potentialWinners.Clear();
    }
}