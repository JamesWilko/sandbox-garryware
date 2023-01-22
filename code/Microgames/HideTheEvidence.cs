using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// All players are given a weapon and have to drop it in a bush or bin before time runs out to win.
/// </summary>
public class HideTheEvidence : Microgame
{
    private readonly ShuffledDeck<Model> binModels = new();
    
    private List<Pistol> weapons;
    
    public HideTheEvidence()
    {
        Rules = MicrogameRules.WinOnTimeout; // Any players who have "evidence" against them will lose in finish, so everybody else should win
        ActionsUsedInGame = PlayerAction.DropWeapon;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 4;
        
        binModels.Add(Model.Load("models/sbox_props/park_bin/park_bin.vmdl"));
        binModels.Add(Model.Load("models/sbox_props/bin/street_bin.vmdl"));
        binModels.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.hide-evidence");
        weapons = GiveWeapon<Pistol>(To.Everyone);

        int hidingSpotsToSpawn = Game.Clients.Count switch
        {
            < 5 => 1,
            < 10 => 2,
            < 15 => 3,
            _ => 4
        };
        for (int i = 0; i < hidingSpotsToSpawn; ++i)
        {
            var spawn = Room.OnFloorSpawnsDeck.Next();
            var binProp = new BreakableProp()
            {
                Position = spawn.Position,
                Rotation = Rotation.FromYaw(Game.Random.Float() * 360.0f),
                Model = binModels.Next(),
                Indestructible = true,
                Static = true,
                PhysicsEnabled = false,
            };
            AutoCleanup(binProp);
            binProp.PhysicsCollision += OnPhysicsCollisionWithBin;
        }
    }

    private void OnPhysicsCollisionWithBin(CollisionEventData collisionData)
    {
        if (collisionData.Other.Entity is Pistol pistol
            && pistol.LastOwner is GarrywarePlayer player)
        {
            Particles.Create("particles/impact.smokepuff.vpcf", pistol.Position).Destroy();
            Sound.FromEntity("sounds/balloon_pop_cute.sound", collisionData.This.Entity);
            pistol.Delete();
        }
    }

    public override void Finish()
    {
        foreach (var pistol in weapons)
        {
            // If the pistol was thrown in the bin it will have been deleted, so check for that
            if(!pistol.IsValid)
                continue;
            
            // If the pistol has an owner or a last owner then it is still either held or in the world, so cause this player to lose
            var owner = (pistol.Owner ?? pistol.LastOwner) as GarrywarePlayer;
            if (owner != null)
            {
                owner.FlagAsRoundLoser();
            }
        }
        
        // @note: the WinOnTimeout rule will make everyone who doesn't have "evidence" against them win.
        // So you can throw your weapon in a bush, a bin, or into another player so they're holding the evidence instead
    }

    public override void Cleanup()
    {
        RemoveAllWeapons();
    }
}