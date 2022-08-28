using Sandbox;

namespace Garryware;

partial class GWFists : Fists
{
    public override float PrimaryRate => 3.0f;

    private bool WasRightPunch = false;

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
        Attack(WasRightPunch);
        WasRightPunch = !WasRightPunch;
    }

    public override void AttackSecondary()
    {
        return;
    }

    private bool MeleeAttack()
    {
        var forward = Owner.EyeRotation.Forward;
        forward = forward.Normal;

        bool hit = false;

        foreach (var tr in TraceMelee(Owner.EyePosition, Owner.EyePosition + forward * 80, 20.0f))
        {
            if (!tr.Entity.IsValid()) continue;

            tr.Surface.DoBulletImpact(tr);

            hit = true;

            if (!IsServer) continue;

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

    [ClientRpc]
    private void OnMeleeMiss(bool leftHand)
    {
        Host.AssertClient();

        ViewModelEntity?.SetAnimParameter("attack_has_hit", false);
        ViewModelEntity?.SetAnimParameter("attack", true);
        ViewModelEntity?.SetAnimParameter("holdtype_attack", leftHand ? 2 : 1);
    }

    [ClientRpc]
    private void OnMeleeHit(bool leftHand)
    {
        Host.AssertClient();

        ViewModelEntity?.SetAnimParameter("attack_has_hit", true);
        ViewModelEntity?.SetAnimParameter("attack", true);
        ViewModelEntity?.SetAnimParameter("holdtype_attack", leftHand ? 2 : 1);
    }
}
