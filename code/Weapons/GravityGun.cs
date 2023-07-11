using Sandbox;
using System;

namespace Garryware;

public partial class GravityGun : Carriable
{
    // @note: if we change the model from the flashlight then the light attachment needs to be updated
    public override string ViewModelPath => "weapons/rust_flashlight/v_rust_flashlight.vmdl";
    private const string ViewModelTracerAttachment = "light";
    private const string WorldModelTracerAttachment = "muzzle";
    private string EffectAttachmentPoint => ViewModelEntity.IsValid() && IsFirstPersonMode ? ViewModelTracerAttachment : WorldModelTracerAttachment;
    
    [Net, Change(nameof(OnValidGrabChanged))] public bool HasValidGrab { get; set; }
    [Net, Predicted] public bool IsAttemptingToPullEntity { get; set; }
    
    public PhysicsBody HeldBody { get; private set; }
    public Vector3 HeldPos { get; private set; }
    public Rotation HeldRot { get; private set; }
    public ModelEntity HeldEntity { get; private set; }
    public Vector3 HoldPos { get; private set; }
    public Rotation HoldRot { get; private set; }

    protected virtual float MaxPullDistance => 1000.0f;
    protected virtual float MaxPushDistance => 200.0f;
    protected virtual float LinearFrequency => 10.0f;
    protected virtual float LinearDampingRatio => 1.0f;
    protected virtual float AngularFrequency => 10.0f;
    protected virtual float AngularDampingRatio => 1.0f;
    protected virtual float PullForce => 20.0f;
    protected virtual float PullRadius => 80.0f;
    protected virtual float PushForce => 1000.0f;
    protected virtual float ThrowForce => 2000.0f;
    protected virtual float HoldDistance => 50.0f;
    protected virtual float AttachDistance => 150.0f;
    protected virtual float DropCooldown => 0.5f;
    protected virtual float BreakLinearForce => 2000.0f;

    private TimeSince timeSinceDrop;

    private Sound pullEntitySoundLoop;
    private Particles pullEntityParticles;
    
    private Entity lastTargetedEntity;
    private TimeSince timeSinceEntityLastTargeted;
    private TimeSince timeSinceEntityPunted;
    
    private const string grabbedTag = "grabbed";

    public override void Spawn()
    {
        base.Spawn();

        Tags.Add("weapon");
        SetModel("weapons/rust_pistol/rust_pistol.vmdl");
    }

    public override void Simulate(IClient client)
    {
        if (Owner is not Player owner)
            return;
        
        if (Input.Down("attack2") && !IsAttemptingToPullEntity)
        {
            IsAttemptingToPullEntity = true;
            if (!HasValidGrab)
            {
                PlayWeaponSound("weapon.gravgun.pull.start");
            }
        }
        else if(!Input.Down("attack2") && IsAttemptingToPullEntity)
        {
            IsAttemptingToPullEntity = false;
        }
        SetGrabParticlesVisible(IsAttemptingToPullEntity || HasValidGrab);

        if (!Game.IsServer)
            return;

        using (Prediction.Off())
        {
            var eyePos = owner.EyePosition;
            var eyeRot = owner.EyeRotation;
            var eyeDir = owner.EyeRotation.Forward;

            HasValidGrab = HeldBody.IsValid() && HeldBody.PhysicsGroup != null;
            if (HeldBody.IsValid() && HeldBody.PhysicsGroup != null)
            {
                if (Input.Pressed("attack1"))
                {
                    PuntGrabbedEntity();
                }
                else if (Input.Pressed("attack2"))
                {
                    GrabEnd();
                }
                else
                {
                    GrabMove(eyePos, eyeDir, eyeRot);
                }

                return;
            }

            if (timeSinceDrop < DropCooldown)
                return;

            if (!TryGetGravityGunTarget(eyePos, eyePos + eyeDir * MaxPullDistance, out var tr))
            {
                return;
            }
            
            var body = tr.Body;
            var modelEnt = tr.Entity as ModelEntity;
            
            if (body.BodyType != PhysicsBodyType.Dynamic)
                return;

            if (Input.Pressed("attack1"))
            {
                if (tr.Distance < MaxPushDistance)
                {
                    PuntTracedEntity(tr);
                }
            }
            else if (Input.Down("attack2"))
            {
                var physicsGroup = tr.Entity.PhysicsGroup;
                if (physicsGroup.BodyCount > 1)
                {
                    body = modelEnt.PhysicsBody;
                    if (!body.IsValid())
                        return;
                }

                var attachPos = body.FindClosestPoint(eyePos);
                if (eyePos.Distance(attachPos) <= AttachDistance)
                {
                    var holdDistance = HoldDistance + attachPos.Distance(body.MassCenter);
                    GrabStart(modelEnt, body, eyePos + eyeDir * holdDistance, eyeRot);
                }
                else
                {
                    physicsGroup.ApplyImpulse(eyeDir * -PullForce, true);
                }

                lastTargetedEntity = tr.Entity;
                timeSinceEntityLastTargeted = 0;
            }
        }
    }
    
    private bool TryGetGravityGunTarget(Vector3 from, Vector3 to, out TraceResult targetResult)
    {
        const float entityTargetPriorityCutoffTime = 0.3f;

        // If we don't have target priority on a particular prop, then see if we're looking
        // directly at a prop that we can affect and use that
        if (timeSinceEntityLastTargeted < entityTargetPriorityCutoffTime)
        {
            var directTrace = Trace.Ray(from, to)
                .UseHitboxes()
                .WithAnyTags("solid")
                .Ignore(this)
                .DynamicOnly()
                .Radius(2.0f)
                .Run();

            if (CanTraceResultEntityBeGravityGunned(directTrace))
            {
                targetResult = directTrace;
                return true;
            }
        }

        // Get all entities as we may be trying to affect entities that we shouldn't be like those
        // which are part of the world and can't move, or those with physics disabled on them
        var results = Trace.Ray(from, to)
            .UseHitboxes()
            .WithAnyTags("solid")
            .Ignore(this)
            .DynamicOnly()
            .Radius(PullRadius)
            .RunAll();

        // Make sure we got some results at all
        if (results == null)
        {
            targetResult = default;
            return false;
        }

        // Create a line along the target points
        var targetLine = new Line(from, to);
        
        double closestResultToTargetLine = float.MaxValue;
        bool foundValidResult = false;
        TraceResult tr = default;

        // Go through every entity we hit
        foreach (var result in results)
        {
            // Do all the checks to make sure we can actually affect this entity
            if(!CanTraceResultEntityBeGravityGunned(result))
                continue;

            // Get the closest entity to the aim point by using distance from the target line
            // This ensures we affect the entities that we're aiming at directly first
            var entDistanceToTargetLine = targetLine.Distance(result.Entity.Position);
            if (entDistanceToTargetLine < closestResultToTargetLine)
            {
                tr = result;
                closestResultToTargetLine = entDistanceToTargetLine;
                foundValidResult = true;
            }
            
            // Do an additional check to see if one of the affected entities was the entity we were affecting previously
            // If it is then we want to prioritize this entity in case there are a bunch of props stacked in a pile so that
            // we pull the correct one out
            if (result.Entity == lastTargetedEntity && timeSinceEntityLastTargeted < entityTargetPriorityCutoffTime)
            {
                tr = result;
                foundValidResult = true;
                break; // This entity has priority, so don't need to check the others 
            }
        }
        
        targetResult = tr;
        return foundValidResult;
    }

    private bool CanTraceResultEntityBeGravityGunned(TraceResult result)
    {
        if (!result.Hit || !result.Body.IsValid() || !result.Entity.IsValid() || result.Entity.IsWorld)
            return false;

        if (result.Entity.PhysicsGroup == null)
            return false;

        var resultModelEnt = result.Entity as ModelEntity;
        if (!resultModelEnt.IsValid())
            return false;

        if (resultModelEnt.Tags.Has(grabbedTag))
            return false;
                
        if(!resultModelEnt.PhysicsEnabled)
            return false;

        return true;
    }
    
    private void Activate()
    {
    }

    private void Deactivate()
    {
        GrabEnd();
    }

    public override void ActiveStart(Entity ent)
    {
        base.ActiveStart(ent);

        if (Game.IsServer)
        {
            Activate();
        }
    }

    public override void ActiveEnd(Entity ent, bool dropped)
    {
        base.ActiveEnd(ent, dropped);

        if (Game.IsServer)
        {
            Deactivate();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (Game.IsServer)
        {
            Deactivate();
        }
    }

    public override void OnCarryDrop(Entity dropper)
    {
    }

    [GameEvent.Physics.PreStep]
    public void OnPrePhysicsStep()
    {
        if (!Game.IsServer)
            return;

        if (!HeldBody.IsValid())
            return;

        if (HeldEntity is Player)
            return;

        var velocity = HeldBody.Velocity;
        Vector3.SmoothDamp(HeldBody.Position, HoldPos, ref velocity, 0.05f, Time.Delta);
        HeldBody.Velocity = velocity;

        var angularVelocity = HeldBody.AngularVelocity;
        Rotation.SmoothDamp(HeldBody.Rotation, HoldRot, ref angularVelocity, 0.05f, Time.Delta);
        HeldBody.AngularVelocity = angularVelocity;
    }

    private void GrabStart(ModelEntity entity, PhysicsBody body, Vector3 grabPos, Rotation grabRot)
    {
        if (!body.IsValid())
            return;

        if (body.PhysicsGroup == null)
            return;

        GrabEnd();

        HeldBody = body;
        HeldPos = HeldBody.LocalMassCenter;
        HeldRot = grabRot.Inverse * HeldBody.Rotation;

        HoldPos = HeldBody.Position;
        HoldRot = HeldBody.Rotation;

        HeldBody.Sleeping = false;
        HeldBody.AutoSleep = false;

        HeldEntity = entity;
        HeldEntity.Tags.Add(grabbedTag);
        // @todo: we don't want to disable ALL collisions, just the one between the player and the held prop. figure out how to do this?
        HeldEntity.EnableAllCollisions = false;

        if (HeldEntity is IGravityGunCallback callbacks)
        {
            callbacks.OnGravityGunPickedUp(new GravityGunInfo()
            {
                Target = HeldEntity,
                Weapon = this,
                Pawn = Owner,
                Instigator = Owner.Client
            });
        }
        
        Client?.Pvs.Add(HeldEntity);
    }

    private void GrabEnd(bool fromPunt = false)
    {
        timeSinceDrop = 0;

        if (HeldBody.IsValid())
        {
            HeldBody.AutoSleep = true;
        }

        if (HeldEntity.IsValid())
        {
            Client?.Pvs.Remove(HeldEntity);
        }

        HeldBody = null;
        HeldRot = Rotation.Identity;

        if (HeldEntity.IsValid())
        {
            if (HeldEntity is IGravityGunCallback callbacks)
            {
                callbacks.OnGravityGunDropped(new GravityGunInfo()
                {
                    Target = HeldEntity,
                    Weapon = this,
                    Pawn = Owner,
                    Instigator = Owner.Client
                });
            }
            
            // @todo: we don't want to disable ALL collisions, just the one between the player and the held prop. figure out how to do this?
            HeldEntity.EnableAllCollisions = true;
            HeldEntity.Tags.Remove(grabbedTag);
            HeldEntity = null;
        }
    }

    private void GrabMove(Vector3 startPos, Vector3 dir, Rotation rot)
    {
        if (!HeldBody.IsValid())
            return;

        var attachPos = HeldBody.FindClosestPoint(startPos);
        var holdDistance = HoldDistance + attachPos.Distance(HeldBody.MassCenter);

        HoldPos = startPos - HeldPos * HeldBody.Rotation + dir * holdDistance;
        HoldRot = rot * HeldRot;
    }

    private void PuntGrabbedEntity()
    {
        var eyeDir = Owner.AimRay.Forward;
        
        if (HeldBody.PhysicsGroup.BodyCount > 1)
        {
            // Don't throw ragdolls as hard
            HeldBody.PhysicsGroup.ApplyImpulse(eyeDir * (ThrowForce * 0.5f), true);
            HeldBody.PhysicsGroup.ApplyAngularImpulse(Vector3.Random * ThrowForce, true);
        }
        else
        {
            HeldBody.ApplyImpulse(eyeDir * (HeldBody.Mass * ThrowForce));
            HeldBody.ApplyAngularImpulse(Vector3.Random * (HeldBody.Mass * ThrowForce));
        }

        var entity = HeldEntity;
        ShootPuntTracer(HeldEntity.WorldSpaceBounds.Center);
        GrabEnd(fromPunt: true);

        if (entity is IGravityGunCallback callbacks)
        {
            callbacks.OnGravityGunPunted(new GravityGunInfo()
            {
                Target = entity,
                Weapon = this,
                Pawn = Owner,
                Instigator = Owner.Client
            });
        }
    }

    private void PuntTracedEntity(TraceResult tr)
    {
        var eyeDir = Owner.AimRay.Forward;
        
        var pushScale = 1.0f - Math.Clamp(tr.Distance / MaxPushDistance, 0.0f, 1.0f);
        tr.Body.ApplyImpulseAt(tr.EndPosition, eyeDir * (tr.Body.Mass * (PushForce * pushScale)));
        ShootPuntTracer(tr.HitPosition);
        
        if (HeldEntity is IGravityGunCallback callbacks)
        {
            callbacks.OnGravityGunPunted(new GravityGunInfo()
            {
                Target = HeldEntity,
                Weapon = this,
                Pawn = Owner,
                Instigator = Owner.Client
            });
        }
    }
    
    public override bool IsUsable(Entity user)
    {
        return Owner == null || HeldBody.IsValid();
    }

    [ClientRpc]
    private void ShootPuntTracer(Vector3 location)
    {
        PlayWeaponSound("weapon.gravgun.punt");
        
        var muzzle = EffectEntity.GetAttachment(EffectAttachmentPoint).GetValueOrDefault();
        var tracer = Particles.Create("particles/gravgun/weapon.gravgun.punt.vpcf", EffectEntity);
        tracer.SetPosition(1, muzzle.Position);
        tracer.SetPosition(2, location);
        tracer.Destroy();
        timeSinceEntityPunted = 0;
    }

    private void OnValidGrabChanged(bool wasValidGrab, bool hasValidGrab)
    {
        if (hasValidGrab)
        {
            pullEntitySoundLoop = PlayWeaponSound("weapon.gravgun.pull.loop");
        }
        else
        {
            pullEntitySoundLoop.Stop();
            
            // @note: lazy hack to stop the end sound playing when punt something directly from holding it
            if (timeSinceEntityPunted > 0.3f)
            {
                PlayWeaponSound("weapon.gravgun.pull.end");
            }
        }
    }

    private void SetGrabParticlesVisible(bool visible)
    {
        if (visible && pullEntityParticles == null)
        {
            pullEntityParticles = Particles.Create("particles/gravgun/weapon.gravgun.pull.vpcf", EffectEntity, EffectAttachmentPoint);
        }
        else if (!visible && pullEntityParticles != null)
        {
            pullEntityParticles?.Destroy();
            pullEntityParticles = null;
        }
    }
    
    private Sound PlayWeaponSound(string eventName)
    {
        if (ViewModelEntity.IsValid() && IsFirstPersonMode)
        {
            return Sound.FromScreen($"{eventName}.local");
        }
        else
        {
            return Sound.FromEntity(eventName, EffectEntity);
        }
    }
    
}