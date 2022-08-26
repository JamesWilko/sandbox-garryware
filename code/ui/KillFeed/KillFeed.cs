using Sandbox;
using Sandbox.UI;

namespace Garryware.UI;

public partial class KillFeed : Panel
{
    public static KillFeed Current;
    
    public KillFeed()
    {
        Current = this;

        StyleSheet.Load("/ui/KillFeed/KillFeed.scss");
        
        GameEvents.OnPlayerLockedInResult += OnPlayerLockedInResult;
    }

    public override void OnDeleted()
    {
        base.OnDeleted();
        
        GameEvents.OnPlayerLockedInResult -= OnPlayerLockedInResult;
    }

    private void OnPlayerLockedInResult(Client player, RoundResult result)
    {
        AddEntry(player.PlayerId, player.Name, result);
    }
    
    public Panel AddEntry(long lsteamid, string left, RoundResult result)
    {
        var e = Current.AddChild<KillFeedEntry>();
        e.SetPlayer(lsteamid, left);
        e.SetResult(result);
        return e;
    }
    
}