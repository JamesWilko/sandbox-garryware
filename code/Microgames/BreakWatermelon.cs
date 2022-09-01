using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players must break a watermelon to win. They can only break one and there are decoy melons that are indestructible.
/// </summary>
public class BreakWatermelon : Microgame
{
    private int breakableMelonsSpawned;
    
    public BreakWatermelon()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("Break a watermelon!");
    }

    public override void Start()
    {
        GiveWeapon<Fists>(To.Everyone);

        
        int breakableMelonsToSpawn = Math.Clamp((int) Math.Ceiling(Client.All.Count * Random.Shared.Float(0.5f, 0.75f)), 1, Client.All.Count);
        int totalMelons = Math.Clamp((int) Math.Ceiling(breakableMelonsToSpawn * Random.Shared.Float(1.5f, 2.5f)), 1, Room.OnBoxSpawns.Count);
        breakableMelonsSpawned = breakableMelonsToSpawn;
        
        for (int i = 0; i < totalMelons; ++i)
        {
            var spawn = Room.OnBoxSpawnsDeck.Next();
            var melon = new Watermelon()
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                CanGib = true,
                Indestructible = i >= breakableMelonsToSpawn 
            };
            AutoCleanup(melon);
            
            melon.OnBroken += OnMelonBroken;
        }
    }
    
    private void OnMelonBroken(BreakableProp crate, Entity attacker)
    {
        if(IsGameFinished())
            return;
        
        if (attacker is GarrywarePlayer player)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
        
        breakableMelonsSpawned--;
        if (breakableMelonsSpawned == 0)
            EarlyFinish();
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        breakableMelonsSpawned = 0;
    }
}