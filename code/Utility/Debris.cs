using System.Collections.Generic;
using Sandbox;

namespace Garryware;

public static class Debris
{
    private static readonly List<Entity> debrisEntities = new();

    public static void Add(Entity entity)
    {
        debrisEntities.Add(entity);
    }

    public static void Add(IEnumerable<Entity> entities)
    {
        debrisEntities.AddRange(entities);
    }

    public static void Clear()
    {
        foreach (var ent in debrisEntities)
        {
            if(ent.IsValid)
                ent.Delete();
        }
        debrisEntities.Clear();
    }

}
