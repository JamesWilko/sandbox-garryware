using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Garryware.UI;

public class NameTags<T> : Panel where T : NameTag, new()
{
    private readonly Dictionary<IClient, T> nameTagsLookup = new();
    
    public override void Tick()
    {
        base.Tick();
        
        // Add name tags for clients that joined
        foreach (var client in Game.Clients.Except(nameTagsLookup.Keys))
        {
            if (ShowTagForClient(client))
            {
                var entry = AddClient(client);
                nameTagsLookup[client] = entry;
            }
        }

        // Remove name tags for clients that left or aren't valid anymore for some reason
        foreach (var client in nameTagsLookup.Keys.Except(Game.Clients))
        {
            RemoveClientTag(client);
        }
        foreach (var client in Game.Clients)
        {
            if (!ShowTagForClient(client))
            {
                RemoveClientTag(client);
            }
        }

        // Order by place, if place hasn't been determined yet then stick them at the end
        foreach (var pair in nameTagsLookup)
        {
            pair.Value.Style.Order = pair.Key.GetInt(Tags.Place, 99);
        }

        // Manually update the name tags after we've deleted any players who shouldn't be shown anymore
        foreach (var tag in nameTagsLookup.Values)
        {
            tag.Update();
        }
    }
    
    protected virtual T AddClient(IClient entry)
    {
        var nameTagPanel = new T();
        nameTagPanel.Client = entry;
        nameTagPanel.Transform = entry.Pawn.Transform;
        return nameTagPanel;
    }

    private bool ShowTagForClient(IClient client)
    {
        return client.IsValid
               && !client.IsOwnedByLocalClient // is not our local player
               && client.Pawn != null && client.Pawn.IsValid; // they have a pawn in the game
    }

    private void RemoveClientTag(IClient client)
    {
        if (nameTagsLookup.TryGetValue(client, out var tag))
        {
            tag?.Delete();
            nameTagsLookup.Remove(client);
        }
    }

}
