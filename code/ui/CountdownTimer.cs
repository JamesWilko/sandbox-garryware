using System;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class CountdownTimer : Panel
{
    private Label CountdownLabel { get; set; }

    private int lastSeconds = 0;

    private const string clockFormat = "{0,2:00.##}:{1,2:00.##}";
    
    public CountdownTimer()
    {
        CountdownLabel = Add.Label();
    }

    public override void Tick()
    {
        base.Tick();

        if (CountdownLabel == null)
            return;

        if (GarrywareGame.Current.IsCountdownTimerEnabled)
        {
            CountdownLabel.Style.Opacity = 1.0f;
            CountdownLabel.Style.FontColor = GetColor();

            var minutes = Math.Max(0, Math.Floor(GarrywareGame.Current.TimeUntilCountdownExpires / 60));
            var seconds = (int) Math.Max(0, Math.Ceiling(GarrywareGame.Current.TimeUntilCountdownExpires - minutes * 60));
            CountdownLabel.SetText(string.Format(clockFormat, minutes, seconds));

            if (seconds != lastSeconds && minutes == 0)
            {
                SoundUtility.PlayCountdown(seconds);
                SoundUtility.PlayClockTick(seconds);
            }
            lastSeconds = seconds;
        }
        else
        {
            CountdownLabel.Style.Opacity = 0.0f;
        }
    }

    private Color GetColor()
    {
        if (GarrywareGame.Current.TimeUntilCountdownExpires <= 3.0f)
        {
            return Color.Red;
        }
        return Color.White;
    }
    
}
