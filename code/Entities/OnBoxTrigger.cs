using Sandbox;
using SandboxEditor;

namespace Garryware.Entities;

[Library("gw_trigger_on_box"), HammerEntity]
[Title("On Box Trigger")]
public class OnBoxTrigger : BaseTrigger
{
    
    public bool Contains(Vector3 position)
    {
        bool withinX = WorldSpaceBounds.Mins.x <= position.x && WorldSpaceBounds.Maxs.x >= position.x;
        bool withinY = WorldSpaceBounds.Mins.y <= position.y && WorldSpaceBounds.Maxs.y >= position.y;
        bool withinZ = WorldSpaceBounds.Mins.z <= position.z && WorldSpaceBounds.Maxs.z >= position.z;
        return withinX && withinY && withinZ;
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
