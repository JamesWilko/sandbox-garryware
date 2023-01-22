using Sandbox;

namespace Garryware.Entities;

[Library("gw_trigger_on_box")]
[Title("On Box Trigger")]
public class OnBoxTrigger : Trigger
{
    
    public override void OnTouchStart(Entity toucher)
    {
        base.OnTouchStart(toucher);
        
        toucher.Tags.Add(Garryware.Tags.OnBox);
    }

    public override void OnTouchEnd(Entity toucher)
    {
        base.OnTouchEnd(toucher);
        
        toucher.Tags.Remove(Garryware.Tags.OnBox);
    }
}
