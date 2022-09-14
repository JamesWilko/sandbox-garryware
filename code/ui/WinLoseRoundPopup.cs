using System;

namespace Garryware.UI;

public class WinLoseRoundPopup : SimplePopup
{

    public void SetResult(RoundResult result)
    {
        Lifetime = 3.0f;
        
        switch (result)
        {
            case RoundResult.Won:
                Text = "#ui.round.win";
                AddClass("Won");
                break;
            case RoundResult.Lost:
                Text = "#ui.round.lost";
                AddClass("Lost");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
}
