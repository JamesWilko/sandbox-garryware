using Sandbox;
using Sandbox.Citizen;

namespace Garryware;

public sealed class GarrywarePawn : Pawn
{
	[Property, Group("Components")] public SkinnedModelRenderer ThirdPersonModel { get; set; }
	[Property, Group("Components")] public CameraComponent FirstPersonCamera { get; set; }
	[Property, Group("Components")] public CharacterController CharacterController { get; set; }
	[Property, Group("Components")] public PlayerMovementStats MovementStats { get; set; }
	[Property, Group("Components")] public CitizenAnimationHelper AnimationHelper { get; set; }
	
	[Sync] public bool IsSprinting { get; set; }
	[Sync] public bool IsWalking { get; set; }
	[Sync] public bool IsCrouching { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	
	private Vector3 desiredAnalogMove;
	private Vector3 desiredVelocity;
	
	protected override void OnPossessed()
	{
		base.OnPossessed();
		
		ThirdPersonModel.RenderType = IsLocallyControlled ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
		FirstPersonCamera.Enabled = IsLocallyControlled;
	}
	
	protected override void OnUpdate()
	{
		base.OnUpdate();
		
		// Update the eye angles from our look input
		if (CharacterController.IsValid() && IsPossessed)
		{
			EyeAngles += Input.AnalogLook;
			EyeAngles = EyeAngles.WithPitch(EyeAngles.pitch.Clamp(-89f, 89f));
		}

		// Rotate player to face the eye angles direction
		{
			var wt = Transform.World;
			wt = wt.WithRotation(Rotation.FromYaw(EyeAngles.yaw));
			Transform.World = wt;
		}

		// Update the third person animations
		if (AnimationHelper.IsValid())
		{
			AnimationHelper.MoveStyle = IsWalking ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;
			AnimationHelper.WithVelocity(CharacterController.Velocity);
			AnimationHelper.WithWishVelocity(desiredVelocity);
			AnimationHelper.WithLook(EyeAngles.Forward);
			AnimationHelper.IsGrounded = CharacterController.IsOnGround;
			AnimationHelper.DuckLevel = IsCrouching ? 1f : 0f; // @todo: smoothing?
			AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None; // @todo: equipment
			AnimationHelper.Handedness = CitizenAnimationHelper.Hand.Both; // @todo: equipment
		}
		
		// Update the first person camera
		if (IsLocallyControlled)
		{
			var wt = FirstPersonCamera.WorldTransform;
			wt = wt.WithRotation(EyeAngles);
			FirstPersonCamera.WorldTransform = wt;
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		
		if (!CharacterController.IsValid())
			return;

		if (!IsLocallyControlled)
			return;

		BuildDesiredInput();
		BuildDesiredVelocity();
		BuildMovementInputs();
		ApplyAcceleration();
		ApplyMovement();
	}

	private void BuildDesiredInput()
	{
		desiredAnalogMove = Input.AnalogMove;
		// @todo: allow freezing players?
	}

	private void BuildDesiredVelocity()
	{
		var direction = desiredAnalogMove * EyeAngles.WithPitch(0f).ToRotation();
		desiredVelocity = direction.WithZ(0f).Normal * GetDesiredSpeed();
	}
	
	private void BuildMovementInputs()
	{
		IsWalking = Input.Down("Walk");
		IsSprinting = Input.Down("Run");
		IsCrouching = Input.Down("Duck");

		if (CharacterController.IsOnGround && Input.Pressed("Jump"))
		{
			// @todo: this sucks ass as a jump, fix it
			CharacterController.Punch(Vector3.Up * MovementStats.JumpVelocity);
		}
	}

	private void ApplyAcceleration()
	{
		CharacterController.Acceleration = GetAcceleration();
	}

	private void ApplyMovement()
	{
		if (CharacterController.IsOnGround)
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0f);
			CharacterController.Accelerate(desiredVelocity);
		}
		else
		{
			var velocity = desiredVelocity.ClampLength(GetMaxAcceleration());
			CharacterController.Accelerate(velocity);
			CharacterController.Accelerate(MovementStats.Gravity);
		}
		
		CharacterController.ApplyFriction(GetFriction());
		CharacterController.Move();
	}

	private float GetDesiredSpeed()
	{
		if (IsCrouching) return MovementStats.CrouchingSpeed;
		if (IsWalking) return MovementStats.WalkingSpeed;
		if (IsSprinting) return MovementStats.SprintingSpeed;
		return MovementStats.RunningSpeed;
	}

	private float GetAcceleration()
	{
		if (!CharacterController.IsOnGround) return MovementStats.AirAcceleration;
		if (IsCrouching) return MovementStats.CrouchingAcceleration;
		if (IsWalking) return MovementStats.WalkingAcceleration;
		if (IsSprinting) return MovementStats.SprintingAcceleration;
		return MovementStats.RunningAcceleration;
	}

	private float GetMaxAcceleration()
	{
		return CharacterController.IsOnGround ? MovementStats.MaxAcceleration : MovementStats.AirMaxAcceleration;
	}

	private float GetFriction()
	{
		if (IsCrouching) return MovementStats.CrouchingFriction;
		if (IsWalking) return MovementStats.WalkingFriction;
		if (IsSprinting) return MovementStats.SprintingFriction;
		return MovementStats.RunningFriction;
	}
	
}
