using Sandbox;

namespace Garryware;

public partial class NpcSmg : NpcWeapon
{
    public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
    public override float PrimaryRate => 8.0f;
    public override float ReloadTime => 5.0f;
    public override bool FiresTracers => true;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_smg/rust_smg.vmdl");
    }
    
    public override void SimulateAnimator(CitizenAnimationHelper anim)
    {
        anim.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
        anim.Handedness = CitizenAnimationHelper.Hand.Right;
        anim.AimBodyWeight = 1.0f;
    }
    
    public override void AttackPrimary()
    {
        base.AttackPrimary();
        (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
        
        ShootEffects();
        var snd = PlaySound("rust_smg.shoot");
        snd.SetVolume(0.33f);
        ShootBullet(GetCurrentSpread(), 1.5f, 9.0f, 3.0f);
    }
    
}
