using Sandbox;

namespace Garryware;

public partial class Pistol : Weapon
{
    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
    public override float RateOfFire => 15.0f;
    public override int MagazineCapacity => 10;
    public override int DefaultAmmoInReserve => 100;

    public override void Spawn()
    {
        base.Spawn();

        SetModel("weapons/rust_pistol/rust_pistol.vmdl");
    }
    
    public override void AttackPrimary()
    {
        base.AttackPrimary();

        (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);

        ShootEffects();
        PlaySound("rust_pistol.shoot");
        ShootBullet(0.05f, 0.1f, 10.0f, 3.0f);
    }
    
}
