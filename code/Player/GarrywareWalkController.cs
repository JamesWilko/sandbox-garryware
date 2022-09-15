using Sandbox;

namespace Garryware;

public partial class GarrywareWalkController : WalkController
{
    [Predicted] public Vector3 KnockbackVelocity { get; protected set; }

    public GarrywareWalkController()
    {
        AirAcceleration = 120f;
    }
    
    public override void Simulate()
    {
        base.Simulate();
        
        // Apply knockback to the players if it has been set
        if (KnockbackVelocity.LengthSquared > 1.0f)
        {
            GroundEntity = null;
            Velocity += KnockbackVelocity;
            Move();
            
            KnockbackVelocity = Vector3.Zero;
        }

        // Set player's ready up state
        if (Input.Pressed(InputButton.Flashlight))
        {
            GarrywareGame.TogglePlayerReadyState();
        }
    }

    public void Knockback(Vector3 velocity)
    {
        KnockbackVelocity = velocity;
        AddEvent("jump");
    }
    
}
