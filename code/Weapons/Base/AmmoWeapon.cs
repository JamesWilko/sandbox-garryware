﻿using Garryware.Entities;
using Sandbox;
using System;
using System.Diagnostics;

namespace Garryware;

public partial class AmmoWeapon : Weapon
{
    [Net, Predicted] public Entity LastOwner { get; set; }
    
    public virtual int MagazineCapacity => 30;
    public virtual int DefaultAmmoInReserve => 120;

    [Net, Predicted] public int AmmoInMagazine { get; set; }
    [Net, Predicted] public int AmmoInReserve { get; set; }

    public virtual bool IsSemiAuto => true;
    
    public delegate void MagazineEmptyDelegate(AmmoWeapon self);
    public event MagazineEmptyDelegate MagazineEmpty;

    public override void Spawn()
    {
        base.Spawn();
        AmmoInMagazine = MagazineCapacity;
        AmmoInReserve = DefaultAmmoInReserve;
    }

    public override bool CanReload()
    {
        if (!base.CanReload()
            || AmmoInReserve <= 0
            || AmmoInMagazine >= MagazineCapacity)
            return false;
        
        return true;
    }

    public override bool CanPrimaryAttack()
    {
        if (IsReloading || AmmoInMagazine <= 0)
        {
            return false;
        }

        return base.CanPrimaryAttack() && (!IsSemiAuto || Input.Pressed("attack1"));
    }

    public bool TakeAmmo(int amount)
    {
        if (AmmoInMagazine < amount)
            return false;

        AmmoInMagazine -= amount;
        if (AmmoInMagazine == 0)
        {
            MagazineEmpty?.Invoke(this);
        }
        return true;
    }

    public override void OnReloadFinish()
    {
        base.OnReloadFinish();
        int ammoAvailable = Math.Min(MagazineCapacity, AmmoInReserve);
        AmmoInMagazine = ammoAvailable;
        AmmoInReserve -= ammoAvailable;
    }

    public override void ShootBullet(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize)
    {
        const float maxDistance = 5000f;
        
        var forward = dir;
        forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;

        // ShootBullet is coded in a way where we can have bullets pass through shit
        // or bounce off shit, in which case it'll return multiple results
        bool hitAnything = false;
        foreach (var tr in TraceBullet(pos, pos + forward * maxDistance, bulletSize))
        {
            hitAnything = true;
            tr.Surface.DoBulletImpact(tr);
            
            if(FiresTracers)
                ShootTracer(tr.EndPosition);
            
            if (!tr.Entity.IsValid()) continue;

            var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 100 * force, damage)
                .UsingTraceResult(tr)
                .WithAttacker(Owner)
                .WithWeapon(this);
            
            // We turn prediction off for this, so any exploding effects don't get culled etc
            using (Prediction.Off())
            {
                tr.Entity.TakeDamage(damageInfo);
            }
        }

        // Fire a tracer that just goes to the max distance if we didn't hit anything
        if (!hitAnything && FiresTracers)
        {
            ShootTracer(pos + forward * maxDistance);
        }
    }
    
}