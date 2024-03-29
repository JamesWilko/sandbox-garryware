﻿using Garryware.Entities;
using Sandbox;
using System;

namespace Garryware.Microgames;

/// <summary>
/// Bouncing balls spawn in and players have to avoid being hit by them.
/// </summary>
public class Dodgeball : Microgame
{
    public Dodgeball()
    {
        Rules = MicrogameRules.WinOnTimeout;
        ActionsUsedInGame = PlayerAction.Punt | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 8.0f;
    }

    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.dodge-balls");
        GiveWeapon<GravityGun>(To.Everyone);

        const int minimumBalls = 5;
        const int maximumBalls = 32;
        var ballsToSpawn = GetRandomAdjustedClientCount(1.2f, 1.5f, minimumBalls, maximumBalls);
        for (int i = 0; i < ballsToSpawn; ++i)
        {
            var spawn = Room.InAirSpawnsDeck.Next();
            var ent = new BouncyBall
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                RenderColor = Color.Random
            };
            ent.ApplyLocalImpulse(Vector3.Random * 1024.0f);
            ent.EntityHit += OnEntityHit;
            AutoCleanup(ent);
        }
    }

    private void OnEntityHit(BouncyBall ball, CollisionEventData eventData)
    {
        if (IsGameFinished())
            return;

        if (eventData.Other.Entity is GarrywarePlayer player)
        {
            player.FlagAsRoundLoser();
            player.RemoveWeapons();
            
            Particles.Create("particles/microgame.confetti.burst.vpcf", player).Destroy();
            Sound.FromEntity("garryware.sfx.confetti.pop", player);
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
