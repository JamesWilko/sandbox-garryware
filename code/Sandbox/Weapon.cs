using Sandbox;
using System.Collections.Generic;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual float ReloadTime => 3.0f;
	public virtual bool FiresTracers => false;
	
	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
		
		EnableAllCollisions = false;
		if (PickupTrigger != null)
		{
			PickupTrigger.EnableAllCollisions = false;
		}
	}

	public override void ActiveEnd(Entity ent, bool dropped)
	{
		base.ActiveEnd(ent, dropped);

		if (!IsValid)
			return;

		EnableAllCollisions = true;
		if (PickupTrigger != null)
		{
			PickupTrigger.EnableTouch = true;
		}
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public override void Simulate( IClient owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}

	public override void CreateViewModel()
	{
		Game.AssertClient();

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
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	[ClientRpc]
	protected virtual void ShootTracer(Vector3 hitLocation)
	{
		Game.AssertClient();
		
		var muzzle = EffectEntity.GetAttachment("muzzle").GetValueOrDefault();
		var tracer = Particles.Create("particles/weapon.tracer.vpcf");
		tracer.SetPosition(0, muzzle.Position);
		tracer.SetPosition(2, hitLocation);
	}
	
	public override IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
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
		const float maxDistance = 5000f;
		
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;
		
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		bool hitAnything = false;
		foreach ( var tr in TraceBullet( pos, pos + forward * maxDistance, bulletSize ) )
		{
			hitAnything = true;
			tr.Surface.DoBulletImpact( tr );
			
			if(FiresTracers)
				ShootTracer(tr.EndPosition);

			if ( !Game.IsServer ) continue;
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
		
		// Fire a tracer that just goes to the max distance if we didn't hit anything
		if (!hitAnything && FiresTracers)
		{
			ShootTracer(pos + forward * maxDistance);
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Game.SetRandomSeed( Time.Tick );
		
		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var ray = Owner.AimRay;
		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( ray.Position, ray.Forward, spread, force / numBullets, damage, bulletSize );
		}
	}
}
