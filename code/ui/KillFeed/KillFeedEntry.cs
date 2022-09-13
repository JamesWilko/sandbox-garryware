using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class KillFeedEntry : Panel
{
    private Label Name { get; set; }
    private Label Result { get; set; }

    private readonly RealTimeSince timeSinceBorn = 0;

    private static readonly string[] WinnerEmojis = { "💪", "😎", "😍", "🤩", "🥰", "😁", "🤑", "🎉", "🎊", "🏆", "🏅", "❤", "✅", "🆒", "✔", "📈" };
    private static readonly string[] LoserEmojis = { "💔", "💤", "💢", "❌", "⛔", "📉", "😥", "😪", "😴", "😭", "😱", "😤", "😩", "😡", "🤬", "🤮" };
    
    public KillFeedEntry()
    {
        Result = Add.Label("", "result");
        Name = Add.Label("", "name");
    }

    public void SetPlayer(long lsteamid, string name)
    {
        Name.Text = name;
        Name.SetClass("me", lsteamid == (Local.Client?.PlayerId));
    }
    
    public void SetResult(RoundResult result)
    {
        AddClass(result == RoundResult.Won ? "won" : "lost");
        Result.Text = Rand.FromArray(result == RoundResult.Won ? WinnerEmojis : LoserEmojis);
    }

    public override void Tick()
    {
        base.Tick();

        if (timeSinceBorn > 5)
        {
            Delete();
        }
    }
}