using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class GameOverScoreboardEntry : Panel
{
    public Client Client;
    private GarrywarePlayer clientPlayer;
    
    private readonly Label place;
    private readonly Label playerName;
    private readonly Label points;
    private readonly Label streakFire;
    private readonly Label currentStreak;
    
    public bool ShowLongestStreak { get; set; }
    
    private RealTimeSince timeSinceUpdate = 0;
    
    public GameOverScoreboardEntry()
    {
        AddClass("entry");

        place = Add.Label("", "place medal");
        playerName = Add.Label("PlayerName", "name");
        points = Add.Label("", "points");
        streakFire = Add.Label("🔥", "fire");
        currentStreak = streakFire.Add.Label("", "streak");
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
        
        var clientPoints = Client.GetInt(Tags.Points);
        var clientStreak = Client.GetInt(ShowLongestStreak ? Tags.MaxStreak : Tags.Streak);
        var clientPlace = Client.GetInt(Tags.Place);
        
        place.Text = UiUtility.GetEmojiForPlace(clientPlace);
        playerName.Text = Client.Name;
        playerName.SetClass("me", Client == Local.Client);
        SetClass("won", clientPlayer?.HasWonRound ?? false);
        SetClass("lost", clientPlayer?.HasLostRound ?? false);
        
        this.points.Text = clientPoints.ToString();
        
        currentStreak.Text = clientStreak.ToString();
        streakFire.Style.Display = clientStreak > 2 ? DisplayMode.Flex : DisplayMode.None;
        
        SetClass("gold", clientPlace == 1);
        SetClass("silver", clientPlace == 2);
        SetClass("bronze", clientPlace == 3);
        
        // Automatically sort this entry when it is placed into a scoreboard
        Style.Order = clientPlace;
        Style.ZIndex = clientPlace; // Increase the z-index as we go so that streaks don't end up under the row above
    }
    
}