using Sandbox;

namespace Garryware;

partial class Fists : Weapon
{
    public override string ViewModelPath => "models/first_person/first_person_arms.vmdl";
    public override float PrimaryRate => 3.0f;

    private bool wasRightPunch = false;

    public override bool CanReload()
    {
        return false;
    }

    private void Attack(bool leftHand)
    {
        if (MeleeAttack())
        {
            OnMeleeHit(leftHand);
        }
        else
        {
            OnMeleeMiss(leftHand);
        }

        (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
    }

    public override void AttackPrimary()
    {
        Attack(wasRightPunch);
        wasRightPunch = !wasRightPunch;
    }

    public override void OnCarryDrop(Entity dropper)
    {
    }

    public override void CreateViewModel()
    {
        Game.AssertClient();

        if (string.IsNullOrEmpty(ViewModelPath))
            return;

        ViewModelEntity = new ViewModel
        {
            Position = Position,
            Owner = Owner,
            EnableViewmodelRendering = true,
            EnableSwingAndBob = false,
        };

        ViewModelEntity.SetModel(ViewModelPath);
        ViewModelEntity.SetAnimGraph("models/first_person/first_person_arms_punching.vanmgrph");
    }

    private bool MeleeAttack()
    {
        var forward = Owner.AimRay.Forward;
        forward = forward.Normal;

        bool hit = false;

        foreach (var tr in TraceMelee(Owner.AimRay.Position, Owner.AimRay.Position + forward * 80, 20.0f))
        {
            if (!tr.Entity.IsValid()) continue;

            tr.Surface.DoBulletImpact(tr);

            hit = true;

            if (!Game.IsServer) continue;

            using (Prediction.Off())
            {
                var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 50, 25)
                    .UsingTraceResult(tr)
                    .WithAttacker(Owner)
                    .WithWeapon(this);

                tr.Entity.TakeDamage(damageInfo);
            }
        }

        return hit;
    }
    
    public override void SimulateAnimator( CitizenAnimationHelper anim )
    {
        anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
        anim.Handedness = CitizenAnimationHelper.Hand.Both;
        anim.AimBodyWeight = 1.0f;
    }

    [ClientRpc]
    private void OnMeleeMiss(bool leftHand)
    {
        Game.AssertClient();

        ViewModelEntity?.SetAnimParameter("attack_has_hit", false);
        ViewModelEntity?.SetAnimParameter("attack", true);
        ViewModelEntity?.SetAnimParameter("holdtype_attack", leftHand ? 2 : 1);
    }

    [ClientRpc]
    private void OnMeleeHit(bool leftHand)
    {
        Game.AssertClient();

        ViewModelEntity?.SetAnimParameter("attack_has_hit", true);
        ViewModelEntity?.SetAnimParameter("attack", true);
        ViewModelEntity?.SetAnimParameter("holdtype_attack", leftHand ? 2 : 1);
    }
}
