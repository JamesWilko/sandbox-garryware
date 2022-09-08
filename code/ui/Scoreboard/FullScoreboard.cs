using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class FullScoreboard : Panel
{
    public Panel Canvas { get; protected set; }
    Dictionary<Client, FullScoreboardEntry> Rows = new();

    public Panel Header { get; protected set; }

    public FullScoreboard()
    {
        StyleSheet.Load("/ui/Scoreboard/FullScoreboard.scss");
        AddClass("scoreboard");

        AddHeader();

        Canvas = Add.Panel("canvas");
    }

    public override void OnHotloaded()
    {
        base.OnHotloaded();

        foreach (var pair in Rows)
        {
            pair.Value.Delete();
        }
        Rows.Clear();
    }

    public override void Tick()
    {
        base.Tick();

        SetClass("open", ShouldBeOpen());

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
    }

    public virtual bool ShouldBeOpen()
    {
        return Input.Down(InputButton.Score);
    }

    protected virtual void AddHeader()
    {
        Header = Add.Panel("header");
        Header.Add.Label("Name", "name");
        Header.Add.Label("Points", "points");
        Header.Add.Label("Streak", "streak");
        Header.Add.Label("Ping", "ping");
    }

    protected virtual FullScoreboardEntry AddClient(Client entry)
    {
        var p = Canvas.AddChild<FullScoreboardEntry>();
        p.Client = entry;
        return p;
    }
}

public partial class FullScoreboardEntry : Panel
{
    public Client Client;

    public Label PlayerName;
    public Label Points;
    public Label Streak;
    public Label Ping;
    public Label Result;

    public FullScoreboardEntry()
    {
        AddClass("entry");

        PlayerName = Add.Label("PlayerName", "name");
        Points = Add.Label("", "points");
        Streak = Add.Label("", "streak");
        Ping = Add.Label("", "ping");
        Result = Add.Label("", "result");
    }

    RealTimeSince TimeSinceUpdate = 0;

    public override void Tick()
    {
        base.Tick();

        if (!IsVisible)
            return;

        if (!Client.IsValid())
            return;

        if (TimeSinceUpdate < 0.1f)
            return;

        TimeSinceUpdate = 0;
        UpdateData();
    }

    public virtual void UpdateData()
    {
        var place = Client.GetInt(Tags.Place, 99);
        
        PlayerName.Text = Client.Name;
        Points.Text = Client.GetInt(Tags.Points).ToString();
        Streak.Text = Client.GetInt(Tags.Streak).ToString();
        Ping.Text = Client.Ping.ToString();
        SetClass("me", Client == Local.Client);
        Style.Order = place;

        switch (GarrywareGame.Current.State)
        {
            // Show how well the player is doing
            default:
                Result.Text = UiUtility.GetEmojiForLockedInResult(Client);
                break;
            
            // Show the players ready-up state next to their name in the scoreboard if they're ready to play
            case GameState.WaitingForPlayers:
            case GameState.StartingSoon:
                Result.Text = Client.GetInt(Tags.IsReady) == 1 ? "👍" : string.Empty;
                break;
        }
    }

    public virtual void UpdateFrom(Client client)
    {
        Client = client;
        UpdateData();
    }
}