using System;
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
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        int numWhiteBalls = GetRandomAdjustedClientCount(0.5f, 0.7f);
        int numDecoyBalls = (int) Math.Ceiling(numWhiteBalls * Rand.Float(2f, 3f));
        Repeat(SpawnWhiteBall, numWhiteBalls);
        Repeat(SpawnDecoyBall, numDecoyBalls);

        GiveWeapon<GravityGun>(To.Everyone);
        ShowInstructions("#microgame.instructions.catch-white");
    }

    private void SpawnWhiteBall()
    {
        var ball = new BouncyBall
        {
            Transform = Room.InAirSpawnsDeck.Next().Transform,
            RenderColor = Color.White
        };
        ball.ApplyLocalImpulse(Vector3.Random * 512.0f);
        AutoCleanup(ball);
        
        ball.Caught += OnWhiteBallCaught;
    }

    private void SpawnDecoyBall()
    {
        var ball = new BouncyBall
        {
            Transform = Room.InAirSpawnsDeck.Next().Transform,
            RenderColor = CommonEntities.ColorsDeck.Next().AsColor() // @note: use colors deck here as it can't generate white
        };
        ball.ApplyLocalImpulse(Vector3.Random * 512.0f);
        AutoCleanup(ball);
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
