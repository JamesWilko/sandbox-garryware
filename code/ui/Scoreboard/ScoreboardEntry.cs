using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class ScoreboardEntry : Panel
{
    public Client Client;

    public int PlaceValue;
    
    public Label Place;
    public Label PlayerName;
    public Label Points;

    public Label StreakFire;
    public Label CurrentStreak;
    
    private RealTimeSince timeSinceUpdate = 0;
    
    private static readonly string[] PlaceEmojis = { "🥇", "🥈", "🥉" };

    public ScoreboardEntry()
    {
        AddClass("entry");

        Place = Add.Label("", "place");
        PlayerName = Add.Label("PlayerName", "name");
        
        Points = Add.Label("", "points");

        StreakFire = Add.Label("🔥", "fire");
        CurrentStreak = StreakFire.Add.Label("", "streak");
    }

    public override void Tick()
    {
        base.Tick();

        if (!IsVisible)
            return;

        if (!Client.IsValid())
            return;

        if (timeSinceUpdate < 0.1f)
            return;

        timeSinceUpdate = 0;
        UpdateData();
    }

    public virtual void UpdateData()
    {
        var points = Client.GetInt("points");
        var streak = Client.GetInt("streak");
        var place = Client.GetInt("place");

        // @todo
        PlaceValue = place;
        Place.Text = place > 0 && place < PlaceEmojis.Length ? PlaceEmojis[place - 1] : string.Empty;
        
        PlayerName.Text = Client.Name;
        SetClass("me", Client == Local.Client);
        // @todo: highlight based on round result
        
        Points.Text = points.ToString();
        
        CurrentStreak.Text = streak.ToString();
        StreakFire.Style.Opacity = streak > 2 ? 1.0f : 0.0f;
    }

    public virtual void UpdateFrom(Client client)
    {
        Client = client;
        UpdateData();
    }
}