using System.Threading.Tasks;
using Sandbox;

namespace Garryware.Microgames;

public class TutorialGame
{
    public async Task Play()
    {
        const float tutorialRoundTimer = 5;
        
        SoundUtility.PlayTutorial();
        
        // @note: don't change room at first since it should take place in the start up room which is the big room with the boxes
        await ShowInstructions("#tutorial.intro.1");
        await ShowInstructions("#tutorial.intro.2");
        await ShowInstructions("#tutorial.intro.3");
        GarrywareGame.Current.AvailableActions = PlayerAction.Jump;
        await ShowInstructions("#tutorial.intro.4");
        
        await ShowInstructions("#microgame.get-ready");
        SoundUtility.PlayNewRound(tutorialRoundTimer);
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.ResetRound();
            }
        }
        
        GameEvents.NewInstructions("#tutorial.get-on-a-box", 3);
        GarrywareGame.Current.SetCountdownTimer(tutorialRoundTimer);
        await GameTask.DelaySeconds(tutorialRoundTimer);
        GarrywareGame.Current.ClearCountdownTimer();
        GarrywareGame.Current.AvailableActions = PlayerAction.None;
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                if (player.IsOnABox())
                {
                    player.FlagAsRoundWinner();
                }
                else
                {
                    player.FlagAsRoundLoser();
                }
            }
        }
        await GameTask.DelaySeconds(0.5f);
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                GameEvents.NewInstructions(To.Single(client), player.HasWonRound ? "#tutorial.get-on-a-box.success" : "#tutorial.get-on-a-box.failure", 3f);
            }
        }
        await GameTask.DelaySeconds(3);
        
        await ShowInstructions("#tutorial.outro.1");
        await ShowInstructions("#tutorial.outro.2");
    }
    
    private async Task ShowInstructions(string text, float displayTime = 3f)
    {
        GameEvents.NewInstructions(text, displayTime);
        await GameTask.DelaySeconds(displayTime); 
    }
    
}
