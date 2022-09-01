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
    private readonly ShuffledDeck<Model> bushModels = new();
    private readonly ShuffledDeck<Model> binModels = new();

    private readonly List<BreakableProp> bushes = new();
    private List<Pistol> weapons;
    
    public HideTheEvidence()
    {
        Rules = MicrogameRules.WinOnTimeout; // Any players who have "evidence" against them will lose in finish, so everybody else should win
        ActionsUsedInGame = PlayerAction.DropWeapon;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 4;
        
        bushModels.Add(Model.Load("models/sbox_props/shrubs/beech/beech_bush_medium.vmdl"));
        bushModels.Add(Model.Load("models/sbox_props/shrubs/beech/beech_bush_regular_medium_b.vmdl"));
        bushModels.Shuffle();
        
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
        
        int hidingSpotsToSpawn = (int)Math.Clamp(Client.All.Count * Rand.Float(0.25f, 0.5f), 1, Room.OnFloorSpawns.Count);
        for (int i = 0; i < hidingSpotsToSpawn; ++i)
        {
            var spawn = Room.OnFloorSpawnsDeck.Next();
            if (Rand.Float() < 0.33f)
            {
                var binProp = new BreakableProp()
                {
                    Position = spawn.Position,
                    Rotation = Rotation.FromYaw(Rand.Float() * 360.0f),
                    Model = binModels.Next(),
                    Indestructible = true,
                    Static = true,
                    PhysicsEnabled = false,
                };
                AutoCleanup(binProp);
                binProp.PhysicsCollision += OnPhysicsCollisionWithBin;
            }
            else
            {
                var bushProp = new BreakableProp()
                {
                    Position = spawn.Position - Vector3.Up * 10.0f,
                    Rotation = Rotation.FromYaw(Rand.Float() * 360.0f),
                    Model = bushModels.Next(),
                    Indestructible = true,
                    Static = true,
                    PhysicsEnabled = false,
                };
                AutoCleanup(bushProp);
                bushes.Add(bushProp);
            }
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
            
            // Determine if the weapon is hidden or not
            var owner = (pistol.Owner ?? pistol.LastOwner) as GarrywarePlayer;
            var isHidden = false;
            
            if (pistol.Owner != null)
            {
                // Player is still holding their weapon, it's not been hidden
                isHidden = false;
            }
            else
            {
                // Check if its close enough to a bush to be considered hidden
                foreach (var bush in bushes)
                {
                    if (Vector3.DistanceBetween(pistol.Position, bush.Position) < 50.0f)
                    {
                        isHidden = true;
                        break;
                    }
                }
            }
            
            // If the weapon hasn't been hidden then cause the player to lose
            if (!isHidden)
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
        bushes.Clear();
    }
}