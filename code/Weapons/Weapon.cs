using System;
using Sandbox;
using System.Collections.Generic;

namespace Garryware;

public partial class Weapon : BaseCarriable, IUse
{
	public virtual float RateOfFire => 5.0f;
	
	public virtual float ReloadTime => 3.0f;
	
	public virtual int MagazineCapacity => 30;
	public virtual int DefaultAmmoInReserve => 120;

	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	
	[Net, Predicted] public int AmmoInMagazine { get; set; }
	[Net, Predicted] public int AmmoInReserve { get; set; }
	
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted] public bool IsReloading { get; set; }

	[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }
	
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add("item");
		
		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		AmmoInMagazine = MagazineCapacity;
		AmmoInReserve = DefaultAmmoInReserve;

		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public virtual bool CanReload()
	{
		if (!Owner.IsValid()
		    || !Input.Down(InputButton.Reload)
		    || AmmoInReserve <= 0
		    || AmmoInMagazine >= MagazineCapacity)
			return false;

		return true;
	}
	
	public virtual void Reload()
	{
		if (IsReloading)
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}
	
	public virtual bool CanPrimaryAttack()
	{
		if (!Owner.IsValid()
		    || !Input.Down(InputButton.PrimaryAttack)
		    || !Input.Pressed(InputButton.PrimaryAttack))
		return false;

		if(IsReloading || AmmoInMagazine <= 0)
		 	return false;
		
		var rate = RateOfFire;
		if (rate <= 0)
			return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}
	
	public virtual void AttackPrimary()
	{
		AmmoInMagazine--;
		TimeSincePrimaryAttack = 0;
	}
	
	public override void Simulate(Client player)
	{
		if (TimeSinceDeployed < 0.6f)
			return;
		
		if (CanReload())
		{
			Reload();
		}
		
		// Reload could have changed our owner
		if (!Owner.IsValid())
			return;

		if (CanPrimaryAttack())
		{
			using (LagCompensation())
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}
		}
		
		// AttackPrimary could have changed our owner
		if (!Owner.IsValid())
			return;
		
		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}
	
	public virtual void OnReloadFinish()
	{
		IsReloading = false;

		int ammoAvailable = Math.Min(MagazineCapacity, AmmoInReserve);
		AmmoInMagazine = ammoAvailable;
		AmmoInReserve -= ammoAvailable;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		var player = user as Player;
		if ( Owner != null ) return false;

		if ( player.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet(Vector3 start, Vector3 end, float radius = 2.0f)
	{
		bool underWater = Trace.TestPoint(start, "water");

		var trace = Trace.Ray(start, end)
			.UseHitboxes()
			.WithAnyTags("solid", "player", "npc")
			.Ignore(this)
			.Size(radius);

		//
		// If we're not underwater then we can hit water
		//
		if (!underWater)
			trace = trace.WithAnyTags("water");

		var tr = trace.Run();

		if (tr.Hit)
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	public IEnumerable<TraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this );

		var tr = trace.Run();

		if ( tr.Hit )
		{
			yield return tr;
		}
		else
		{
			trace = trace.Size( radius );

			tr = trace.Run();

			if ( tr.Hit )
			{
				yield return tr;
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Rand.SetSeed( Time.Tick );
		ShootBullet( Owner.EyePosition, Owner.EyeRotation.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var pos = Owner.EyePosition;
		var dir = Owner.EyeRotation.Forward;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize );
		}
	}
	
	public override Sound PlaySound(string soundName, string attachment)
	{
		if (Owner.IsValid())
			return Owner.PlaySound(soundName, attachment);

		return base.PlaySound(soundName, attachment);
	}
	
}
