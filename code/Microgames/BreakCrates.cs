using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class BreakCrates : Microgame
{
    private int cratesSpawned;
    
    public BreakCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.UseWeapon;
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

    // Finish the game early if all crates get destroyed
    protected override bool IsGameFinished()
    {
        return base.IsGameFinished() || cratesSpawned == 0;
    }
    
    private void OnCrateDestroyed(Entity attacker)
    {
        if (attacker is GarrywarePlayer player)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
        cratesSpawned--;
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}
