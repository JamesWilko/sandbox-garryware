namespace Garryware.Entities;

public class FloatingTarget : BreakableProp
{
    public override void Spawn()
    {
        // @todo: get a better target model than the ball
        Model = Sandbox.Model.Load("models/ball/ball.vmdl");
        Indestructible = true;
        
        base.Spawn();
    }

    protected override void SetupPhysics()
    {
        base.SetupPhysics();
        
        // Set up physics properties so it floats around the arena
        PhysicsBody.LinearDrag = 0.5f;
        PhysicsBody.DragEnabled = true;
        PhysicsBody.GravityEnabled = false;
        PhysicsBody.Mass = 1000.0f;

        // Start it moving about a little bit by default
        PhysicsBody.Velocity = Vector3.Random * 400.0f;
    }
}