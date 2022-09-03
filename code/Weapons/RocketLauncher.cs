using Sandbox;

namespace Garryware;

public partial class RocketLauncher : ProjectileWeapon<RocketProjectile>
{
    public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";
    public override float PrimaryRate => 2.0f;
    public override int MagazineCapacity => 1;
    public override float ProjectileLaunchSpeed => 2200.0f;
    
    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_shotgun/rust_shotgun.vmdl");
    }
    
    public override bool CanReload()
    {
        // Only one shot in the rocket launcher
        return false;
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
        
        PlaySound("rust_pumpshotgun.shootdouble");
    }

}
