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
        
        RootPanel.StyleSheet.Load("/ui/GarrywareHud.scss");

        // Build
        RootPanel.AddChild<NameTags<NameTag>>();
        RootPanel.AddChild<KillFeed>();
        RootPanel.AddChild<Crosshair>();
        RootPanel.AddChild<GameOver>();
        RootPanel.AddChild<WaitingForPlayers>();
        RootPanel.AddChild<CountdownTimer>();
        RootPanel.AddChild<OnScreenControls>();
        RootPanel.AddChild<AmmoCounter>();
        RootPanel.AddChild<MiniScoreboard<MiniScoreboardEntry>>();
        RootPanel.AddChild<FullScoreboard>();
        RootPanel.AddChild<GameOverScreen>();
        
        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<VoiceList>();
        RootPanel.AddChild<VoiceSpeaker>();
        
        // Listen to game events
        GameEvents.OnNewInstructions += OnNewInstructions;
        GameEvents.OnClearInstructions += OnClearInstructions;
        GameEvents.OnPlayerLockedInResult += OnPlayerLockedInResult;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unbind game events
        GameEvents.OnNewInstructions -= OnNewInstructions;
        GameEvents.OnClearInstructions -= OnClearInstructions;
        GameEvents.OnPlayerLockedInResult -= OnPlayerLockedInResult;
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

    private void OnClearInstructions(string text, float displayTime)
    {
        if (CurrentInstructions?.IsValid ?? false)
        {
            CurrentInstructions.Delete();
        }
    }

    private void OnAvailableControlsUpdated(PlayerAction availableActions)
    {
        // @todo: update on screen controls list
    }
    
    private void OnPlayerLockedInResult(Client player, RoundResult result)
    {
        if (player.IsOwnedByLocalClient)
        {
            var resultPopup = RootPanel.AddChild<WinLoseRoundPopup>();
            resultPopup.SetResult(result);
        }
    }

}
