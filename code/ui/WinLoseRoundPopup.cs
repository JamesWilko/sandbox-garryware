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
                Text = "You won!";
                AddClass("Won");
                break;
            case RoundResult.Lost:
                Text = "You failed!";
                AddClass("Lost");
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
}
