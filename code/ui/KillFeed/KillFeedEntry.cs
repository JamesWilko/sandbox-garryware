using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class KillFeedEntry : Panel
{
    private Label Left { get; set; }
    private Label Method { get; set; }

    private readonly RealTimeSince timeSinceBorn = 0;

    private static readonly string[] WinnerEmojis = { "💪", "😎", "😍", "🤩", "🥰", "😁", "🤑", "🎉", "🎊", "🏆", "🏅", "❤", "✅", "🆒", "✔", "📈" };
    private static readonly string[] LoserEmojis = { "💔", "💤", "💢", "❌", "⛔", "📉", "😥", "😪", "😴", "😭", "😱", "😤", "😩", "😡", "🤬", "🤮" };
    
    public KillFeedEntry()
    {
        Left = Add.Label("", "left");
        Method = Add.Label("", "method");
    }

    public void SetPlayer(long lsteamid, string name)
    {
        Left.Text = name;
        Left.SetClass("me", lsteamid == (Local.Client?.PlayerId));
    }
    
    public void SetResult(RoundResult result)
    {
        AddClass(result == RoundResult.Won ? "Won" : "Lost");
        Method.Text = Rand.FromArray(result == RoundResult.Won ? WinnerEmojis : LoserEmojis);
    }

    public override void Tick()
    {
        base.Tick();

        if (timeSinceBorn > 4)
        {
            Delete();
        }
    }
}