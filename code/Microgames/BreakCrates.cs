using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class BreakCrates : Microgame
{
    private int cratesSpawned;
    
    public BreakCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.UseWeapon;
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("Break a crate!");
    }

    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
        
        // @todo
        cratesSpawned = Client.All.Count; // Math.Clamp(Client.All.Count - Random.Shared.Next(0, 3), 2, Client.All.Count);
        for (int i = 0; i < cratesSpawned; ++i)
        {
            var spawn = CommonEntities.OnBoxSpawnsDeck.Next();
            var ent = new BreakableProp
            {
                Position = spawn.Position,
                Rotation = spawn.Rotation,
                Model = CommonEntities.Crate,
                CanGib = false
            };
            AutoCleanup(ent);
            
            ent.OnBroken += OnCrateDestroyed;
        }
    }
    
    private void OnCrateDestroyed(Entity attacker)
    {
        if (attacker is GarrywarePlayer player)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
        
        cratesSpawned--;
        if (cratesSpawned == 0)
            EarlyFinish();
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}
