using Sandbox;
using Sandbox.UI;

namespace Garryware.UI;

[UseTemplate]
public class GameOverScreen : Panel
{
    public Panel RoundResults { get; set; }
    public Label MedalLabel { get; set; }
    public Label ResultLabel { get; set; }
    public Label PointsLabel { get; set; }
    public Panel ScoreboardContainer { get; set; }
    public Label ReturnToLobbyTimer { get; set; }
    
    public GameOverScreen()
    {
        StyleSheet.Load("/ui/GameOver/GameOverScreen.scss");
        
        var place = Local.Client.GetInt(Tags.Place, 99);
        var points = Local.Client.GetInt(Tags.Points, 0);
        
        // Update player place and results
        MedalLabel.Text = UiUtility.GetEmojiForPlace(place);
        ResultLabel.Text = $"You came {UiUtility.GetPlaceQualifier(place)}!"; // @localization
        PointsLabel.Text = $"{points} Points"; // @localization
        BuildRoundResults();
        BuildEndOfGameScoreboard();
        Show();
    }

    private async void Show()
    {
        await GameTask.Yield();
        SetClass("open", true);
    }

    public override void Tick()
    {
        base.Tick();

        if (Input.Pressed(InputButton.Menu))
        {
            SetClass("open", !HasClass("open"));
        }

        if(!IsVisible)
            return;

        // Tick the countdown timer down
        if (GarrywareGame.Current.IsCountdownTimerEnabled)
        {
            ReturnToLobbyTimer.Text = $"Returning to lobby in {GarrywareGame.Current.TimeUntilCountdownExpires.Relative:N0} seconds..."; // @localization
        }
        else
        {
            ReturnToLobbyTimer.Text = "Returning to lobby shortly..."; // @localization
        }
    }
    
    private void BuildRoundResults()
    {
        RoundResults.DeleteChildren();
        
        var perRoundResults = Local.Client.GetValue(Tags.PerRoundResults, string.Empty);
        for (int i = 0; i < perRoundResults.Length; ++i)
        {
            // @note: we add a separate entry for every character so that we can animate everything individually eventually, even if we're not doing that yet
            var entry = RoundResults.AddChild<RoundResultEntry>();
            entry.Text = $"{perRoundResults[i]}";
        }
    }

    private void BuildEndOfGameScoreboard()
    {
        ScoreboardContainer.DeleteChildren();

        foreach (var client in Client.All)
        {
            var entry = ScoreboardContainer.AddChild<GameOverScoreboardEntry>();
            entry.Client = client;
            entry.ShowLongestStreak = true;
            entry.UpdateData();
        }
        
    }
    
}
