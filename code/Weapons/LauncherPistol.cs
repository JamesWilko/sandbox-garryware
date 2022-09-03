using Sandbox;

namespace Garryware;

partial class LauncherPistol : AmmoWeapon
{
    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    public override float PrimaryRate => 0.5f;

    public override int MagazineCapacity => 1;

    private readonly ShuffledDeck<GameColor> beamColors;
    
    public LauncherPistol()
    {
        beamColors = new ShuffledDeck<GameColor>();
        beamColors.Add(GameColor.Red);
        beamColors.Add(GameColor.Green);
        beamColors.Add(GameColor.Magenta);
        beamColors.Add(GameColor.Yellow);
        beamColors.Add(GameColor.Cyan);
        beamColors.Shuffle();
    }

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
        ShootBullet(0f, 1.5f, 0f, 3.0f);
        TakeAmmo(1);
    }
    
    protected override void ShootEffects()
    {
        base.ShootEffects();
        if(IsServer) return;
        
        foreach (var tr in TraceBullet(Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 5000))
        {
            if (!tr.Entity.IsValid())
                continue;
            
            var muzzle = EffectEntity.GetAttachment("muzzle").GetValueOrDefault();
            var fx = Particles.Create("particles/launcher.beam.vpcf");
            fx.SetPosition(0, muzzle.Position);
            fx.SetPosition(1, tr.HitPosition);
            fx.SetPosition(2, beamColors.Next().AsColor());
            fx.SetPosition(3, new Vector3(7f, 1f, 0f));
            fx.Destroy();
            break;
        }
    }
    
    public override void ShootBullet(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize)
    {
        var forward = dir;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;
        
        foreach (var tr in TraceBullet(pos, pos + forward * 5000, bulletSize))
        {
            if (!IsServer) continue;
            if (!tr.Entity.IsValid()) continue;
            
            if (tr.Entity is Player player)
            {
                if (player.Controller is GarrywareWalkController controller)
                {
                    controller.Knockback(Vector3.Up * 700.0f);
                }
            }
            else
            {
                using (Prediction.Off())
                    tr.Entity.ApplyAbsoluteImpulse(Vector3.Up * 500.0f);
            }

        }
    }
    
}
