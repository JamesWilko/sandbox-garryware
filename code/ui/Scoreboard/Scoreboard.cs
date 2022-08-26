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

        // @todo: remove
        SetClass("open", true);
        if (!IsVisible)
            return;
        
        // Clients that were added
        foreach (var client in Client.All.Except(Rows.Keys))
        {
            var entry = AddClient(client);
            Rows[client] = entry;
        }

        foreach (var client in Rows.Keys.Except(Client.All))
        {
            if (Rows.TryGetValue(client, out var row))
            {
                row?.Delete();
                Rows.Remove(client);
            }
        }
        
        // Order by points
        // @todo
        SortChildren((a, b) =>
        {
            var aEntry = a as ScoreboardEntry;
            var bEntry = a as ScoreboardEntry;
            return aEntry.PlaceValue.CompareTo(bEntry.PlaceValue);
        });
        
    }
    
    protected virtual T AddClient(Client entry)
    {
        var p = Canvas.AddChild<T>();
        p.Client = entry;
        return p;
    }
}