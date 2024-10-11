using Sandbox;

namespace Garryware;

public class PlayerMovementStats : Component
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3(0, 0, -800);
	[Property] public float JumpVelocity { get; set; } = 400f;

	[Property] public float RunningSpeed { get; set; } = 200f;
	[Property] public float SprintingSpeed { get; set; } = 300f;
	[Property] public float WalkingSpeed { get; set; } = 100f;
	[Property] public float CrouchingSpeed { get; set; } = 100f;

	[Property] public float RunningAcceleration { get; set; } = 10f;
	[Property] public float AirAcceleration { get; set; } = 20f;
	[Property] public float WalkingAcceleration { get; set; } = 10f;
	[Property] public float CrouchingAcceleration { get; set; } = 10f;
	[Property] public float SprintingAcceleration { get; set; } = 10f;

	[Property] public float MaxAcceleration { get; set; } = 10f;
	[Property] public float AirMaxAcceleration { get; set; } = 100f;

	[Property] public float RunningFriction { get; set; } = 8f;
	[Property] public float SprintingFriction { get; set; } = 4f;
	[Property] public float WalkingFriction { get; set; } = 4f;
	[Property] public float CrouchingFriction { get; set; } = 4f;
}
