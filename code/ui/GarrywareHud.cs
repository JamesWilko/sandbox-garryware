using Sandbox;
using Sandbox.UI;

[Library]
public partial class GarrywareHud : HudEntity<RootPanel>
{
    public GarrywareHud()
    {
        if ( !IsClient )
            return;

        RootPanel.StyleSheet.Load( "/ui/GarrywareHud.scss" );

        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<VoiceList>();
        RootPanel.AddChild<VoiceSpeaker>();
        RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
    }
}