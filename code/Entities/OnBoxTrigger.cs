using Sandbox;
using SandboxEditor;

namespace Garryware.Entities;

[Library("gw_trigger_on_box"), HammerEntity]
[Title("On Box Trigger")]
public class OnBoxTrigger : BaseTrigger
{
    
    public bool Contains(Entity entity)
    {
        return WorldSpaceBounds.Overlaps(entity.WorldSpaceBounds);
    }

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
