using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class AvoidTheLight : Microgame
{
    private List<FloatingSpotlight> spotlights = new();
    private readonly ShuffledDeck<Vector3> directions = new();
    private bool avoidLight = true;
    
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
        avoidLight = Game.Random.Float() > 0.5f;
        ShowInstructions(avoidLight ? "#microgame.instructions.avoid-light" : "#microgame.instructions.stay-in-light");

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
            AutoCleanup(spotlight);
            spotlights.Add(spotlight);
            
            if (avoidLight)
            {
                spotlight.ApplyAbsoluteImpulse(directions.Next() * Game.Random.Float(300f, 500f));
            }
            else
            {
                spotlight.ApplyAbsoluteImpulse(directions.Next() * Game.Random.Float(200f, 400f));
            }
        }
    }

    public override void Start()
    {
        GiveWeapon<BallLauncher>(To.Everyone);
    }

    private bool IsPlayerInLight(GarrywarePlayer player)
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

        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player && !player.HasLockedInResult)
            {
                if (avoidLight && IsPlayerInLight(player))
                {
                    player.FlagAsRoundLoser();
                }
                else if (!avoidLight && !IsPlayerInLight(player))
                {
                    player.FlagAsRoundLoser();
                }
            }
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