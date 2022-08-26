using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Garryware.UI;

public partial class Scoreboard<T> : Panel where T : ScoreboardEntry, new()
{
    public Panel Canvas { get; protected set; }
    Dictionary<Client, T> Rows = new();

    public Panel Header { get; protected set; }

    public Scoreboard()
    {
        StyleSheet.Load("/ui/Scoreboard/Scoreboard.scss");
        AddClass("scoreboard");

        Canvas = Add.Panel("canvas");
    }

    public override void Tick()
    {
        base.Tick();
        
        // Add rows for clients that joined
        foreach (var client in Client.All.Except(Rows.Keys))
        {
            var entry = AddClient(client);
            Rows[client] = entry;
        }

        // Remove rows for clients that left
        foreach (var client in Rows.Keys.Except(Client.All))
        {
            if (Rows.TryGetValue(client, out var row))
            {
                row?.Delete();
                Rows.Remove(client);
            }
        }
        
        // Order by place, if place hasn't been determined yet then stick them at the end
        foreach (var pair in Rows)
        {
            pair.Value.Style.Order = pair.Key.GetInt(Tags.Place, 99);
        }
    }
    
    protected virtual T AddClient(Client entry)
    {
        var p = Canvas.AddChild<T>();
        p.Client = entry;
        return p;
    }
}