using Garryware.Entities;
using Sandbox;
using System;

namespace Garryware;

partial class AmmoWeapon : Weapon
{
    public virtual int MagazineCapacity => 30;
    public virtual int DefaultAmmoInReserve => 120;

    [Net, Predicted] public int AmmoInMagazine { get; set; }
    [Net, Predicted] public int AmmoInReserve { get; set; }

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

        return base.CanPrimaryAttack();
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
}

