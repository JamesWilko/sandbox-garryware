using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class PickupThatCan : Microgame
{
    private static readonly Model BinModel = Model.Load("models/sbox_props/park_bin/park_bin.vmdl");
    private static readonly Model CanModel = Model.Load("models/citizen_props/sodacan01.vmdl");

    public PickupThatCan()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 6;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.pickup-that-can");
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

        // Spawn a load of cans
        // @note: since the cans are small and hard to lose, we want to spawn a lot of them
        int numCans = (int) (Client.All.Count * Rand.Float(2.0f, 5.0f));
        for (int i = 0; i < numCans; ++i)
        {
            var can = new BreakableProp
            {
                Transform = Room.InAirSpawnsDeck.Next().Transform,
                Model = CanModel,
                Indestructible = true,
            };
            AutoCleanup(can);
            can.ApplyLocalImpulse(Vector3.Random * 1024.0f);
        }
    }

    private void OnPhysicsCollisionWithBin(CollisionEventData collisionData)
    {
        if (collisionData.Other.Entity is BreakableProp prop
            && prop.ClientLastPickedUpBy != null
            && prop.ClientLastPickedUpBy.IsValid()
            && prop.ClientLastPickedUpBy.Pawn is GarrywarePlayer player) 
        {
            player.FlagAsRoundWinner();

            Particles.Create("particles/impact.smokepuff.vpcf", prop.Position).Destroy();
            Sound.FromEntity("sounds/balloon_pop_cute.sound", collisionData.This.Entity);
            prop.Delete();
        }
    }
    
    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}