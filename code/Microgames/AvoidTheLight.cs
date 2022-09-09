using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class AvoidTheLight : Microgame
{
    private List<FloatingSpotlight> spotlights = new();
    private readonly ShuffledDeck<Vector3> directions = new();

    protected virtual float MinSpeed { get; set; } = 250.0f;
    protected virtual float MaxSpeed { get; set; } = 450.0f;
    
    public AvoidTheLight()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Sprint | PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.DarkRoom };
        WarmupLength = 3.5f;
        GameLength = 8f;
        
        directions.Add(new (1f, 1f));
        directions.Add(new (-1f, 1f));
        directions.Add(new (1f, -1f));
        directions.Add(new (-1f, -1f));
        directions.Add(new (0.5f, 1f));
        directions.Add(new (-0.5f, 1f));
        directions.Add(new (0.5f, -1f));
        directions.Add(new (-0.5f, -1f));
        directions.Add(new (1f, 0.5f));
        directions.Add(new (-1f, 0.5f));
        directions.Add(new (1f, -0.5f));
        directions.Add(new (-1f, -0.5f));
        directions.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.avoid-light");

        int numLights = Room.Size switch
        {
            RoomSize.Small => 2,
            RoomSize.Medium => 4,
            RoomSize.Large => 6,
            _ => throw new ArgumentOutOfRangeException()
        };

        spotlights.Clear();
        for (int i = 0; i < numLights; ++i)
        {
            var spotlight = new FloatingSpotlight()
            {
                Position = Room.AboveBoxSpawnsDeck.Next().Position,
                Rotation = new Angles(90f, 0, 0).ToRotation()
            };
            spotlight.ApplyAbsoluteImpulse(directions.Next() * Rand.Float(MinSpeed, MaxSpeed));
            AutoCleanup(spotlight);
            spotlights.Add(spotlight);
        }
    }

    public override void Start()
    {
        GiveWeapon<BallLauncher>(To.Everyone);
    }

    protected bool IsPlayerInLight(GarrywarePlayer player)
    {
        foreach (var spotlight in spotlights)
        {
            var distance = Vector3.DistanceBetween(player.Position, spotlight.Position.WithZ(player.Position.z));
            if (distance < 220f)
            {
                return true;
            }
        }
        return false;
    }
    
    public override void Tick()
    {
        base.Tick();

        foreach (var client in Client.All)
        {
            if(client.Pawn is GarrywarePlayer player && !player.HasLockedInResult && IsPlayerInLight(player))
                player.FlagAsRoundWinner();
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