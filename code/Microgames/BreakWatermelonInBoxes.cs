﻿using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A bunch of crates spawn that have various props in them. Players have to break a crate that has a watermelon inside it.
/// </summary>
public class BreakWatermelonInBoxes : Microgame
{
    private readonly List<BreakableProp> props = new();
    private readonly Dictionary<BreakableProp, BreakableProp> crates = new();
    private readonly ShuffledDeck<Model> decoyModels = new();

    public BreakWatermelonInBoxes()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 5;
        
        decoyModels.Add(CommonEntities.BeachBall);
        decoyModels.Add(Cloud.Model("facepunch.fireextinguisher"));
        decoyModels.Add(Cloud.Model("facepunch.traffic_cone"));
        decoyModels.Add(Cloud.Model("facepunch.beer_cask"));
        decoyModels.Add(Cloud.Model("facepunch.wet_floor_sign"));
        decoyModels.Shuffle();
    }
    
    public override void Setup()
    {
        int melonsToSpawn = GetRandomAdjustedClientCount(0.5f, 0.75f);
        int totalSpawns = Math.Clamp((int) Math.Ceiling(melonsToSpawn * Random.Shared.Float(2.0f, 3.0f)), 1, Room.OnBoxSpawns.Count);
        
        // Create the melon spawns
        for (int i = 0; i < melonsToSpawn; ++i)
        {
            var melon = new Watermelon()
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                PhysicsEnabled = false
            };
            AutoCleanup(melon);
            props.Add(melon);
        }

        // Then spawn in the decoy props
        for (int i = melonsToSpawn; i < totalSpawns; ++i)
        {
            var decoy = new BreakableProp()
            {
                Transform = Room.AboveBoxSpawnsDeck.Next().Transform,
                PhysicsEnabled = false,
                Model = decoyModels.Next()
            };
            AutoCleanup(decoy);
            props.Add(decoy);
        }

        // Put a crate over every prop and make them transparent so we can see whats in each
        foreach (var prop in props)
        {
            var crate = new BreakableProp()
            {
                Position = prop.Position - Vector3.Up * 5.0f,
                PhysicsEnabled = false,
                Model = CommonEntities.Crate,
                RenderColor = Color.White.WithAlpha(0.5f),
            };
            AutoCleanup(crate);
            crate.Scale = 1.0f;
            
            crate.OnBroken += OnBreakCrate;
            
            crates.Add(crate, prop);
        }
        
        ShowInstructions("#microgame.look-carefully");
    }
    
    public override void Start()
    {
        // Make the crates opaque
        foreach (var crate in crates.Keys)
        {
            crate.RenderColor = Color.White;
        }
            
        GiveWeapon<Fists>(To.Everyone);
        ShowInstructions("#microgame.instructions.find-watermelon");
    }
    
    private void OnBreakCrate(BreakableProp crate, Entity attacker)
    {
        if(attacker is GarrywarePlayer player
           && !player.HasLockedInResult
           && crates.TryGetValue(crate, out var prop))
        {
            if (prop is Watermelon)
            {
                player.FlagAsRoundWinner();
                player.RemoveWeapons();
            }

            prop.PhysicsEnabled = true;
            prop.ApplyLocalImpulse(Vector3.Random * 400.0f);
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        props.Clear();
    }
    
}
