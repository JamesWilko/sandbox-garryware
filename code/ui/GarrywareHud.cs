using Garryware;
using Garryware.UI;
using Sandbox;
using Sandbox.UI;

[Library]
public partial class GarrywareHud : HudEntity<RootPanel>
{
    private InstructionsPopup CurrentInstructions { get; set; }
    
    public GarrywareHud()
    {
        if ( !IsClient )
            return;
        
        RootPanel.StyleSheet.Load( "/ui/GarrywareHud.scss" );

        // Build
        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<VoiceList>();
        RootPanel.AddChild<VoiceSpeaker>();
        RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();

        RootPanel.AddChild<Crosshair>();
        RootPanel.AddChild<WaitingForPlayers>();
        RootPanel.AddChild<CountdownTimer>();
        
        // Listen to game events
        GarrywareGame.Current.OnNewInstructions += OnNewInstructions;
        GarrywareGame.Current.OnAvailableControlsUpdated += OnAvailableControlsUpdated;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unbind game events
        if (GarrywareGame.Current != null)
        {
            GarrywareGame.Current.OnNewInstructions -= OnNewInstructions;
        }
    }
    
    private async void OnNewInstructions(string text, float displayTime)
    {
        // Remove the old instructions
        if (CurrentInstructions?.IsValid ?? false)
        {
            CurrentInstructions.Delete();
        }
        
        // Show the new popup
        CurrentInstructions = RootPanel.AddChild<InstructionsPopup>();
        CurrentInstructions.Text = text;
        CurrentInstructions.Lifetime = displayTime;
    }
    
    private void OnAvailableControlsUpdated(PlayerAction availableActions)
    {
        // @todo: update on screen controls list
    }

}
