using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class BreakCrates : Microgame
{
    public BreakCrates()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.UseWeapon;
    }
    
    public override void Setup()
    {
        GiveWeapon<Pistol>(To.Everyone);
    }

    public override void Start()
    {
        // @todo
        var numCratesToSpawn = Client.All.Count; // Math.Clamp(Client.All.Count - Random.Shared.Next(0, 3), 2, Client.All.Count);
        for (int i = 0; i < numCratesToSpawn; ++i)
        {
            var spawn = OnBoxSpawnsDeck.Next();
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
            RemoveWeapons(To.Single(player));
        }
    }

    public override void Finish()
    {
        RemoveWeapons(To.Everyone);
    }

    public override void Cleanup()
    {
    }
}
