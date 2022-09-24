using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class AmmoCounter : Panel
{
    private Label ammoCountLabel;
    
    public AmmoCounter()
    {
        ammoCountLabel = Add.Label();
    }

    public override void Tick()
    {
        base.Tick();

        if (Local.Pawn is GarrywarePlayer player
            && player.Inventory.Active is AmmoWeapon weapon)
        {
            Style.Display = DisplayMode.Flex;
            ammoCountLabel.Text = weapon.AmmoInMagazine.ToString("N0");
            ammoCountLabel.SetClass("low-ammo", weapon.AmmoInMagazine > 0 && weapon.AmmoInMagazine < weapon.MagazineCapacity * 0.5f);
            ammoCountLabel.SetClass("out-of-ammo", weapon.AmmoInMagazine <= 0);
        }
        else
        {
            Style.Display = DisplayMode.None;
        }
    }
}
