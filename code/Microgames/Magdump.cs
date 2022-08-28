using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players have to shoot all of their ammo before time runs out or they lose.
/// </summary>
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
        GiveWeapon<GWPistol>(To.Everyone);
        // @todo: listen to weapon run out of ammo event and cause player to win
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
}
