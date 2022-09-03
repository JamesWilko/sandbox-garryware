using Sandbox;

namespace Garryware;

partial class Pistol : AmmoWeapon
{
    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    public override float PrimaryRate => 15.0f;

    public override int MagazineCapacity => 10;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_pistol/rust_pistol.vmdl");
    }

    public override bool CanPrimaryAttack()
    {
        return base.CanPrimaryAttack() && Input.Pressed(InputButton.PrimaryAttack);
    }

    public override void AttackPrimary()
    {
        (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);

        ShootEffects();
        PlaySound("rust_pistol.shoot");
        ShootBullet(0.02f, 1.5f, 9.0f, 3.0f);
        TakeAmmo(1);
    }
}
