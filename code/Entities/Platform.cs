using Sandbox;

namespace Garryware.Entities;

public class Platform : BreakableProp
{
    private static readonly Model PlatformModel = Model.Load("models/props/beam_railing_a/metal_beam_base_a.vmdl");
    
    public override void Spawn()
    {
        Static = true;
        Indestructible = true;
        
        base.Spawn();
        
        Rotation = new Angles(180f, 0f, 0f).ToRotation();
        Model = PlatformModel;
        Scale = 12.0f;
    }
}
