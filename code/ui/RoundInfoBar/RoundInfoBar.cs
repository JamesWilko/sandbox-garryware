using System;
using Sandbox.UI;

namespace Garryware.UI;

[UseTemplate]
public class RoundInfoBar : Panel
{
    
    public Label CountdownLabel { get; set; }
    public Label SubCountdownLabel { get; set; }
    public Label RoundLabel { get; set; }
    public Label WinnersCountLabel { get; set; }
    public Label LosersCountLabel { get; set; }

    private int lastSeconds;
    
    public RoundInfoBar()
    {
        StyleSheet.Load("/ui/RoundInfoBar/RoundInfoBar.scss");
    }

    public override void Tick()
    {
        base.Tick();

        TickCountdown();

        switch (GarrywareGame.Current.State)
        {
            case GameState.WaitingForPlayers:
                RoundLabel.Text = "#ui.bar.waiting-for-players";
                break;
            case GameState.StartingSoon:
                RoundLabel.Text = "#ui.bar.starting-soon";
                break;
            case GameState.Instructions:
                RoundLabel.Text = "#ui.bar.instructions";
                break;
            case GameState.Playing:
                RoundLabel.Text = string.Format("Round {0:N0}", GarrywareGame.Current.CurrentRound); // @localization
                break;
            case GameState.GameOver:
                RoundLabel.Text = "#ui.bar.game-over";
                break;
            case GameState.Dev:
                RoundLabel.Text = "developer mode";
                break;
        }
        
        WinnersCountLabel.Text = GarrywareGame.Current.NumberOfWinners.ToString();
        LosersCountLabel.Text = GarrywareGame.Current.NumberOfLosers.ToString();
    }

    private void TickCountdown()
    {
        if (GarrywareGame.Current.IsCountdownTimerEnabled)
        {
            var seconds = (int) Math.Max(0, Math.Ceiling(GarrywareGame.Current.TimeUntilCountdownExpires));
            CountdownLabel.Text = Math.Max(seconds - 1, 0).ToString();
            CountdownLabel.SetClass("critical", seconds <= 5);
            
            bool showMilliseconds = GarrywareGame.Current.TimeUntilCountdownExpires < 10 && GarrywareGame.Current.TimeUntilCountdownExpires > 0;
            SubCountdownLabel.Style.Display = showMilliseconds ? DisplayMode.Flex : DisplayMode.None; 
            if (SubCountdownLabel.IsVisible)
            {
                var milliseconds = 100 - Math.Clamp(Math.Abs(GarrywareGame.Current.TimeUntilCountdownExpires - seconds) * 100, 0, 100);
                SubCountdownLabel.Text = milliseconds.ToString("00");
                SubCountdownLabel.SetClass("critical", seconds <= 5);
            }
            
            if (seconds != lastSeconds)
            {
                SoundUtility.PlayCountdown(seconds);
                SoundUtility.PlayClockTick(seconds);
            }
            lastSeconds = seconds;
        }
        else
        {
            CountdownLabel.Text = "--";
            CountdownLabel.SetClass("critical", false);
            SubCountdownLabel.Style.Display = DisplayMode.None;
        }
    }

}
