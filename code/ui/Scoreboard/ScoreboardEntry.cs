using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class ScoreboardEntry : Panel
{
    public Client Client;
    private GarrywarePlayer clientPlayer;
    
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
        if (clientPlayer == null || !clientPlayer.IsValid)
        {
            clientPlayer = Client.Pawn as GarrywarePlayer;
        }
        
        var points = Client.GetInt(Tags.Points);
        var streak = Client.GetInt(Tags.Streak);
        var place = Client.GetInt(Tags.Place);
        
        switch (GarrywareGame.Current.State)
        {
            // Show how well the player is doing
            default:
                Place.Text = place > 0 && place < PlaceEmojis.Length ? PlaceEmojis[place - 1] : string.Empty;
                break;
            
            // Show the players ready-up state next to their name in the scoreboard if they're ready to play
            case GameState.WaitingForPlayers:
            case GameState.StartingSoon:
                Place.Text = Client.GetInt(Tags.IsReady) == 1 ? "👍" : string.Empty;
                break;
        }

        PlayerName.Text = Client.Name;
        SetClass("me", Client == Local.Client);
        SetClass("won", clientPlayer?.HasWonRound ?? false);
        SetClass("lost", clientPlayer?.HasLostRound ?? false);
        
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