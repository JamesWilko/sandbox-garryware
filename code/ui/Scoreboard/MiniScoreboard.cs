using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Garryware.UI;

/// <summary>
/// Smaller scoreboard for the top left corner of the screen.
/// Shows who's in the lead and our current position in relation to them.
/// </summary>
public partial class MiniScoreboard<T> : Panel where T : MiniScoreboardEntry, new()
{
    public Panel Canvas { get; protected set; }
    Dictionary<Client, T> Rows = new();

    private Panel Header { get; set; }
    private Label RoundCount { get; set; }

    public MiniScoreboard()
    {
        StyleSheet.Load("/ui/Scoreboard/MiniScoreboard.scss");
        AddClass("mini-scoreboard");

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
            var client = pair.Key;
            var entry = pair.Value;
            
            // Only show the top 3 players, and us
            // If we're not in the top 3 then put a little gap between us and the top 3 
            var place = client.GetInt(Tags.Place, 99);
            if (place > 3 && !client.IsOwnedByLocalClient)
            {
                place = 99;
            }
            
            entry.Style.Order = place;
            entry.Style.ZIndex = place; // Increase the z-index as we go so that streaks don't end up under the row above
            entry.Style.Opacity = (place > 3 && !client.IsOwnedByLocalClient) ? 0 : 1;
            entry.SetClass("fifth-or-worse", client.IsOwnedByLocalClient && place > 4);
            entry.SetClass("gold", place == 1);
            entry.SetClass("silver", place == 2);
            entry.SetClass("bronze", place == 3);
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