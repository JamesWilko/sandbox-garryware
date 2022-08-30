using Sandbox;

namespace Garryware.Entities;

public abstract class Trigger : BaseTrigger
{
    
    public bool ContainsEntity(Entity entity)
    {
        return WorldSpaceBounds.Overlaps(entity.WorldSpaceBounds);
    }
    
}