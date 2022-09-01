using Garryware.Entities;
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
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.SecondaryAttack;
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

        var minimumBalls = 5;
        var maximumBalls = 32;
        var ballsToSpawn = Math.Clamp(Math.Ceiling(Client.All.Count * 1.2f), minimumBalls, maximumBalls);
        for (int i = 0; i < ballsToSpawn; ++i)
        {
            var spawn = Room.InAirSpawnsDeck.Next();
            var ent = new BouncyBall
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation
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
            // TODO: Probably spew some particles or something
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
