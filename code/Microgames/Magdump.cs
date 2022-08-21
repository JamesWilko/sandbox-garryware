using Sandbox;

namespace Garryware.Microgames;

public class Magdump : Microgame
{
    public override void Setup()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.UseWeapon;
        GameLength = 3.0f;
    }

    public override void Start()
    {
        GiveWeapon<Pistol>(To.Everyone);
    }

    public override void Finish()
    {
        RemoveWeapons(To.Everyone);
    }

    public override void Cleanup()
    {
    }
}
