using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Garryware.UI;

public class NameTags<T> : Panel where T : NameTag, new()
{
    private readonly Dictionary<Client, T> rows = new();
    
    public NameTags()
    {
    }

    public override void Tick()
    {
        base.Tick();
        
        // Add name tags for clients that joined
        foreach (var client in Client.All.Except(rows.Keys))
        {
            if (!client.IsOwnedByLocalClient)
            {
                var entry = AddClient(client);
                rows[client] = entry;
            }
        }

        // Remove name tags for clients that left
        foreach (var client in rows.Keys.Except(Client.All))
        {
            if (rows.TryGetValue(client, out var row))
            {
                row?.Delete();
                rows.Remove(client);
            }
        }
        
        // Order by place, if place hasn't been determined yet then stick them at the end
        foreach (var pair in rows)
        {
            pair.Value.Style.Order = pair.Key.GetInt(Tags.Place, 99);
        }
    }
    
    protected virtual T AddClient(Client entry)
    {
        var nameTagPanel = new T();
        nameTagPanel.Client = entry;
        nameTagPanel.Transform = entry.Pawn.Transform;
        return nameTagPanel;
    }
    
}
