using Sandbox;
using Sandbox.UI;

namespace Garryware.UI;

public partial class KillFeed : Panel
{
    public static KillFeed Current;

    private static readonly string[] WinnerEmojis = { "💪", "😎", "😍", "🤩", "🥰", "😁", "🤑", "🎉", "🎊", "🏆", "🏅", "❤", "✅", "🆒", "✔", "📈" };
    private static readonly string[] LoserEmojis = { "💔", "💤", "💢", "❌", "⛔", "📉", "😥", "😪", "😴", "😭", "😱", "😤", "😩", "😡", "🤬", "🤮" };
    
    public KillFeed()
    {
        Current = this;

        StyleSheet.Load("/ui/KillFeed/KillFeed.scss");
    }

    public Panel AddEntry(long lsteamid, string left, RoundResult result)
    {
        var e = Current.AddChild<KillFeedEntry>();
        e.AddClass(result == RoundResult.Won ? "Won" : "Lost");
        e.Left.Text = left;
        e.Left.SetClass("me", lsteamid == (Local.Client?.PlayerId));
        e.Method.Text = Rand.FromArray(result == RoundResult.Won ? WinnerEmojis : LoserEmojis);
        return e;
    }
    
}