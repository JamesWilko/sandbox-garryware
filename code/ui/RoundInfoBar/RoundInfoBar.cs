using System;
using System.Collections.Generic;
using Sandbox;
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
    
    public Panel WinnersFeed { get; set; }
    public Panel LosersFeed { get; set; }

    private Queue<Client> winnersQueue = new();
    private RealTimeSince lastWinnerAdded;
    
    private Queue<Client> losersQueue = new();
    private RealTimeSince lastLoserAdded;

    private int lastSeconds;
    
    public RoundInfoBar()
    {
        StyleSheet.Load("/ui/RoundInfoBar/RoundInfoBar.scss");
        
        GameEvents.OnPlayerLockedInResult += OnPlayerLockedInResult;
    }

    public override void OnDeleted()
    {
        base.OnDeleted();
        
        GameEvents.OnPlayerLockedInResult -= OnPlayerLockedInResult;
    }

    public override void Tick()
    {
        base.Tick();
        
        TickCountdown();

        // Update the little text below the timer that usually shows the round
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
        
        // Update count on the hud
        WinnersCountLabel.Text = GarrywareGame.Current.NumberOfWinners.ToString();
        LosersCountLabel.Text = GarrywareGame.Current.NumberOfLosers.ToString();
        
        // Add the kill feed entries with a small delay between them so we don't get overlapping names
        if (lastWinnerAdded > 0.35f && winnersQueue.Count > 0)
        {
            AddKillfeedEntry(winnersQueue.Dequeue(), RoundResult.Won);
            lastWinnerAdded = 0;
        }
        if (lastLoserAdded > 0.35f && losersQueue.Count > 0)
        {
            AddKillfeedEntry(losersQueue.Dequeue(), RoundResult.Lost);
            lastLoserAdded = 0;
        }
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

    private void OnPlayerLockedInResult(Client player, RoundResult result)
    {
        if(result == RoundResult.Won)
            winnersQueue.Enqueue(player);
        else
            losersQueue.Enqueue(player);
    }
    
    private void AddKillfeedEntry(Client player, RoundResult result)
    {
        var feed = result == RoundResult.Won ? WinnersFeed : LosersFeed;
        var e = feed.AddChild<KillFeedEntry>();
        e.SetPlayer(player.PlayerId, player.Name);
        e.SetResult(result);
        e.Style.Order = feed.ChildrenCount * -1;
    }

    public override void OnHotloaded()
    {
        base.OnHotloaded();
        WinnersFeed.DeleteChildren();
        LosersFeed.DeleteChildren();
    }
}
