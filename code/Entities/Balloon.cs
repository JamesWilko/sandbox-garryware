using Sandbox;

namespace Garryware.Entities;

public partial class Balloon : BreakableProp
{
    private static float GravityScale => -0.12f;
    
    [Net] public bool AutoPop { get; set; }
    [Net] public TimeUntil TimeUntilPop { get; set; }

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/citizen_props/balloonregular01.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Dynamic, false);
        PhysicsBody.GravityScale = GravityScale;
        RenderColor = Color.Random;
    }
    
    public override void OnKilled()
    {
        base.OnKilled();
        PlaySound("balloon_pop_cute");
    }

    [Event.Physics.PostStep]
    protected void UpdateGravity()
    {
        if (!this.IsValid())
            return;

        var body = PhysicsBody;
        if (!body.IsValid())
            return;
        
        body.GravityScale = GravityScale;
    }

    [Event.Tick.Server]
    protected void UpdateLifetime()
    {
        if (AutoPop && TimeUntilPop <= 0.0f)
        {
            TakeDamage(DamageInfo.Generic(100f));
        }
    }
    
}