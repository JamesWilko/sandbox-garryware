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

    private Panel Header { get; set; }
    private Label RoundCount { get; set; }

    public Scoreboard()
    {
        StyleSheet.Load("/ui/Scoreboard/Scoreboard.scss");
        AddClass("scoreboard");

        AddHeader();
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
        
        // Update the round count
        RoundCount.Text = GarrywareGame.Current.CurrentRound.ToString("N0");
    }
    
    protected virtual void AddHeader() 
    {
        Header = Add.Panel( "header" );
        Header.Add.Label( "#ui.round", "round" );
        RoundCount = Header.Add.Label( "0", "roundcount" );
    }

    
    protected virtual T AddClient(Client entry)
    {
        var p = Canvas.AddChild<T>();
        p.Client = entry;
        return p;
    }
}