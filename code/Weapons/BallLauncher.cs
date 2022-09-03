using Garryware.Entities;
using Sandbox;

namespace Garryware;

public partial class BallLauncher : ProjectileWeapon<BouncyBall>
{
    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
    public override float PrimaryRate => 2.0f;
    public override int MagazineCapacity => 10;
    
    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_pistol/rust_pistol.vmdl");
    }
    
    public override void AttackPrimary()
    {
        base.AttackPrimary();
        
        PlaySound("rust_pistol.shoot");
    }
    
}
