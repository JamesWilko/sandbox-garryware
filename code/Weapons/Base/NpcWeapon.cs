using System;
using Sandbox;

namespace Garryware;

public partial class NpcWeapon : Weapon
{
    public virtual float MaxSpread { get; } = 1f;
    public virtual float MinSpread { get; } = 0.05f;
    public virtual float FocusFireTime { get; } = 3.0f;
    public virtual float FocusFireDelay { get; } = 1.5f;

    private TimeSince timeSinceStartedShootingThisTarget;
    
    public override bool CanPrimaryAttack()
    {
        if (!Owner.IsValid())
            return false;

        var rate = PrimaryRate;
        if (rate <= 0)
            return true;

        return TimeSincePrimaryAttack > (1 / rate);
    }
    
    public override void AttackPrimary()
    {
        base.AttackPrimary();
        TimeSincePrimaryAttack = 0;
    }

    // @note: this really should be an event, but we're only used on one weapon and by one npc right now so fuck it
    public void OnChoseNewTarget()
    {
        timeSinceStartedShootingThisTarget = 0;
    }

    /// <summary>
    /// Get the current spread for this NPCs weapon. They start off firing wildly and focus fire on their target over time.
    /// </summary>
    public float GetCurrentSpread()
    {
        if (timeSinceStartedShootingThisTarget < FocusFireDelay)
            return MaxSpread;

        float accuracy = Math.Clamp((timeSinceStartedShootingThisTarget - FocusFireDelay) / FocusFireTime, 0f, 1f);
        return MathX.Lerp(MaxSpread, MinSpread, accuracy);
    }
    
}