﻿using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A bunch of bouncy balls spawn in, players have to catch the white ball which can only be caught once.
/// </summary>
public class CatchWhiteBall : Microgame
{

    public CatchWhiteBall()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Punt | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        int whiteBalls = GetRandomAdjustedClientCount(0.5f, 0.7f);
        int decoyBalls = (int) Math.Ceiling(whiteBalls * Game.Random.Float(2f, 3f));

        // Spawn the target balls
        for (int i = 0; i < whiteBalls; ++i)
        {
            var ball = new BouncyBall
            {
                Position = Room.AboveBoxSpawnsDeck.Next().Position,
                Rotation = Rotation.Random,
                GameColor = GameColor.White
            };
            ball.ApplyLocalImpulse(Vector3.Random.WithZ(0).Normal * 512.0f);
            AutoCleanup(ball);
            
            ball.Caught += OnWhiteBallCaught;
        }
        
        // Spawn the decoys
        for (int i = 0; i < whiteBalls; ++i)
        {
            var ball = new BouncyBall
            {
                Position = Room.AboveBoxSpawnsDeck.Next().Position,
                Rotation = Rotation.Random,
                GameColor = CommonEntities.ColorsDeck.Next() // @note: use colors deck here as it can't generate white
            };
            ball.ApplyLocalImpulse(Vector3.Random.WithZ(0).Normal * 512.0f);
            AutoCleanup(ball);
        }
        
        // Set the players off
        GiveWeapon<GravityGun>(To.Everyone);
        ShowInstructions("#microgame.instructions.catch-white");
    }
    
    private void OnWhiteBallCaught(BouncyBall ball, GravityGunInfo info)
    {
        if(ball.RenderColor != Color.White || IsGameFinished())
            return;
        
        if (info.Pawn is GarrywarePlayer player)
        {
            player.FlagAsRoundWinner();
            ball.RenderColor = Color.Red;
        }
    }
    
    public override void Finish()
    {
    }

    public override void Cleanup()
    {
    }
}
