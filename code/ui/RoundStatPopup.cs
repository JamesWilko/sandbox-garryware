using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class RoundStatPopup : Panel
{
    private readonly Label emojiLabel;
    private readonly Label textLabel;
    
    private readonly RealTimeSince timeSinceCreated = 0;
    
    public RoundStatPopup()
    {
        emojiLabel = Add.Label(string.Empty, "emoji");
        textLabel = Add.Label(string.Empty, "text");
    }
    
    public void SetDetails(RoundStat stat, Client subject)
    {
        switch (stat)
        {
            case RoundStat.XWasTheFastestToWin:
                emojiLabel.Text = "🏆";
                textLabel.Text = string.Format("{0} won the round first!", subject.Name); // @localization
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void SetDetails(RoundStat stat, int value)
    {
        switch (stat)
        {
            case RoundStat.EverybodyWon:
                emojiLabel.Text = "🎉";
                textLabel.Text = "#stats.everybody-won";
                break;
            case RoundStat.EverybodyLost:
                emojiLabel.Text = "😤";
                textLabel.Text = "#stats.everybody-lost";
                break;
            case RoundStat.OnlyXPeopleWon:
                emojiLabel.Text = "😮";
                textLabel.Text = string.Format(value == 1 ? "Only {0} person won that round!" : "Only {0} people won that round!", value); // @localization
                break;
            case RoundStat.LowPercentPeopleWon:
                emojiLabel.Text = "😮";
                textLabel.Text = string.Format("Only {0}% of the players won that round!", value); // @localization
                break;
            case RoundStat.HighPercentPeopleWon:
                emojiLabel.Text = "😍";
                textLabel.Text = string.Format("{0}% of the players won that round!", value); // @localization
                break;
            case RoundStat.YouHitTheTargetXTimes:
                emojiLabel.Text = "🎯";
                textLabel.Text = string.Format("You hit the target {0} times!", value); // @localization
                break;
            case RoundStat.YouHitTheTargetXTimes_Failed:
                emojiLabel.Text = "🤔";
                textLabel.Text = string.Format("You hit the target {0} times!", value); // @localization
                break;
            case RoundStat.YouOnlyHitTheTargetXTimes:
                emojiLabel.Text = "🤔";
                textLabel.Text = string.Format("You only hit the target {0} times!", value); // @localization
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public override void Tick()
    {
        base.Tick();

        if (timeSinceCreated > 4f)
        {
            Delete();
        }
    }
    
}
