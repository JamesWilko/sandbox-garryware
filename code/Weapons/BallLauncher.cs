using Garryware.Entities;
using Sandbox;

namespace Garryware;

public partial class BallLauncher : ProjectileWeapon<KnockbackBall>
{
    public override string ViewModelPath => "weapons/rust_crossbow/v_rust_crossbow.vmdl";
    public override float PrimaryRate => 2.0f;
    public override int MagazineCapacity => 1;
    public override float ProjectileLaunchSpeed => 1500.0f;
    public override float LaunchDistanceOffset => 40.0f;
    
    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_crossbow/rust_crossbow.vmdl");
    }
    
    public override void SimulateAnimator(CitizenAnimationHelper anim)
    {
        anim.HoldType = CitizenAnimationHelper.HoldTypes.Shotgun;
        anim.Handedness = CitizenAnimationHelper.Hand.Right;
        anim.AimBodyWeight = 1.0f;
    }
    
    public override void AttackPrimary()
    {
        base.AttackPrimary();
        
        PlaySound("rust_crossbow.shoot");
    }
    
}
