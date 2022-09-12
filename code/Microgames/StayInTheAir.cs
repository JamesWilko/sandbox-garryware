using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class StayInTheAir : Microgame
{
    private ShuffledDeck<float> difficultyDeck;
    private List<KnockbackPlatform> platforms = new();
    
    public StayInTheAir()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.None;
        AcceptableRooms = new[] { MicrogameRoom.Empty };
        WarmupLength = 3f;
        GameLength = 7f;

        difficultyDeck = new ShuffledDeck<float>();
        difficultyDeck.Add(0.35f, 1);
        difficultyDeck.Add(0.4f, 2);
        difficultyDeck.Add(0.5f, 4);
        difficultyDeck.Add(0.6f, 1);
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.stay-in-the-air");
        
        int numPlatforms = (int)Math.Ceiling(Room.OnFloorSpawns.Count * difficultyDeck.Next());
        for (int i = 0; i < numPlatforms; ++i)
        {
            var platform = new KnockbackPlatform()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position + Vector3.Up * 2f,
                GameColor = GetRandomColor()
            };
            AutoCleanup(platform);
            platforms.Add(platform);
        }
    }

    public override void Start()
    {
        GiveWeapon<RocketLauncher>(To.Everyone);
    }

    public override void Tick()
    {
        base.Tick();

        // Anyone who lands on the ground should lose
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player
                && !player.HasLockedInResult
                && player.GroundEntity != null && player.GroundEntity.IsWorld)
            {
                player.FlagAsRoundLoser();
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        
        foreach (var platform in platforms)
        {
            platform.Delete();
        }
        platforms.Clear();
    }

    public override void Cleanup()
    {
    }
}