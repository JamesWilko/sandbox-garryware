using System;
using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class AlarmingCrates : Microgame
{
    private ShuffledDeck<BreakableProp> cratesDeck = new();
    private Dictionary<BreakableProp, BreakableProp> alarms = new();
    private Dictionary<BreakableProp, Sound> alarmSounds = new();

    public AlarmingCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 7;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        int alarmsToSpawn = GetRandomAdjustedClientCount(0.5f, 0.75f);
        int cratesToSpawn = (int) Math.Min(Math.Ceiling(alarmsToSpawn * Random.Shared.Float(1.5f, 2.5f)), Room.AboveBoxSpawns.Count);
        
        // Spawn a bunch of random crates
        for (int i = 0; i < cratesToSpawn; ++i)
        {
            var spawn = Room.AboveBoxSpawnsDeck.Next();
            var crate = new BreakableProp
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                Model = CommonEntities.Crate,
                PhysicsEnabled = false
            };
            AutoCleanup(crate);
            cratesDeck.Add(crate);
            
            crate.OnBroken += OnCrateBroken;
        }
        
        // Hide the alarms inside random crates
        cratesDeck.Shuffle();
        for (int i = 0; i < alarmsToSpawn; ++i)
        {
            var spawnCrate = cratesDeck.Next();
            var alarmProp = new BreakableProp
            {
                Position = spawnCrate.Position + Vector3.Up * 5,
                Rotation = spawnCrate.Rotation,
                Model = CommonEntities.Ball,
                PhysicsEnabled = false
            };
            AutoCleanup(alarmProp);
            alarms.Add(spawnCrate, alarmProp);

            var sound = Sound.FromEntity(To.Everyone, "garryware.sfx.alarm", alarmProp);
            alarmSounds.Add(alarmProp, sound);
        }
        
        ShowInstructions("#microgame.instructions.shut-it-off");
        GiveWeapon<Fists>(To.Everyone);
    }
    
    private void OnCrateBroken(BreakableProp crate, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            if(alarms.TryGetValue(crate, out var alarmProp))
            {
                player.FlagAsRoundWinner();
                player.RemoveWeapons();
                
                alarmProp.PhysicsEnabled = true;
                alarmProp.ApplyLocalImpulse(Vector3.Random * 200.0f);

                if (alarmSounds.TryGetValue(alarmProp, out Sound sound))
                {
                    // @todo: play a cutoff sound here
                    sound.Stop(To.Everyone);
                }
            }
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
        foreach (var sound in alarmSounds.Values)
        {
            sound.Stop(To.Everyone);
        }
    }

    public override void Cleanup()
    {
        cratesDeck.Clear();
        alarms.Clear();
        alarmSounds.Clear();
    }
}