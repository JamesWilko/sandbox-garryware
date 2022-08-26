using Garryware;
using Garryware.UI;
using Sandbox;
using Sandbox.UI;

[Library]
public partial class GarrywareHud : HudEntity<RootPanel>
{
    private SimplePopup CurrentInstructions { get; set; }
    
    public GarrywareHud()
    {
        if (!IsClient)
            return;
        
        RootPanel.StyleSheet.Load( "/ui/GarrywareHud.scss" );

        // Build
        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<VoiceList>();
        RootPanel.AddChild<VoiceSpeaker>();
        RootPanel.AddChild<Garryware.UI.Scoreboard<Garryware.UI.ScoreboardEntry>>();

        RootPanel.AddChild<KillFeed>();
        RootPanel.AddChild<Crosshair>();
        RootPanel.AddChild<GameOver>();
        RootPanel.AddChild<WaitingForPlayers>();
        RootPanel.AddChild<CountdownTimer>();
        
        // Listen to game events
        GarrywareGame.Current.OnNewInstructions += OnNewInstructions;
        GarrywareGame.Current.OnAvailableControlsUpdated += OnAvailableControlsUpdated;
        GarrywareGame.Current.OnRoundResult += OnRoundResult;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unbind game events
        if (GarrywareGame.Current != null)
        {
            GarrywareGame.Current.OnNewInstructions -= OnNewInstructions;
            GarrywareGame.Current.OnAvailableControlsUpdated -= OnAvailableControlsUpdated;
            GarrywareGame.Current.OnRoundResult -= OnRoundResult;
        }
    }
    
    private void OnNewInstructions(string text, float displayTime)
    {
        // Remove the old instructions
        if (CurrentInstructions?.IsValid ?? false)
        {
            CurrentInstructions.Delete();
        }
        
        // Don't show empty instructions, just clear the existing ones
        if(text.Length < 1 || displayTime <= 0.0f)
            return;
        
        // Show the new popup
        CurrentInstructions = RootPanel.AddChild<InstructionsPopup>();
        CurrentInstructions.Text = text;
        CurrentInstructions.Lifetime = displayTime;
    }
    
    private void OnAvailableControlsUpdated(PlayerAction availableActions)
    {
        // @todo: update on screen controls list
    }
    
    private void OnRoundResult(RoundResult result)
    {
        var resultPopup = RootPanel.AddChild<WinLoseRoundPopup>();
        resultPopup.SetResult(result);
    }

}
