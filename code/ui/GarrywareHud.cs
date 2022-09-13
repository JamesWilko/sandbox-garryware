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
        RootPanel.AddChild<Crosshair>();
        RootPanel.AddChild<GameOver>();
        RootPanel.AddChild<WaitingForPlayers>();
        RootPanel.AddChild<OnScreenControls>();
        RootPanel.AddChild<AmmoCounter>();
        RootPanel.AddChild<FullScoreboard>();
        RootPanel.AddChild<GameOverScreen>();
        RootPanel.AddChild<RoundInfoBar>();
        
        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<VoiceList>();
        RootPanel.AddChild<VoiceSpeaker>();

        // Listen to game events
        GameEvents.OnNewInstructions += OnNewInstructions;
        GameEvents.OnClearInstructions += OnClearInstructions;
        GameEvents.OnPlayerLockedInResult += OnPlayerLockedInResult;
        GameEvents.ClientStatReceived += OnClientStatReceived;
        GameEvents.IntegerStatReceived += OnIntegerStatReceived;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unbind game events
        GameEvents.OnNewInstructions -= OnNewInstructions;
        GameEvents.OnClearInstructions -= OnClearInstructions;
        GameEvents.OnPlayerLockedInResult -= OnPlayerLockedInResult;
        GameEvents.ClientStatReceived -= OnClientStatReceived;
        GameEvents.IntegerStatReceived -= OnIntegerStatReceived;
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

    private void OnPlayerLockedInResult(Client player, RoundResult result)
    {
        if (player.IsOwnedByLocalClient)
        {
            var resultPopup = RootPanel.AddChild<WinLoseRoundPopup>();
            resultPopup.SetResult(result);
        }
    }
    
    private void OnClientStatReceived(RoundStat stat, Client subject)
    {
        var statPopup = RootPanel.AddChild<RoundStatPopup>();
        statPopup.SetDetails(stat, subject);
    }

    private void OnIntegerStatReceived(RoundStat stat, int value)
    {
        var statPopup = RootPanel.AddChild<RoundStatPopup>();
        statPopup.SetDetails(stat, value);
    }
    
}
