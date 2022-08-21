
using System.Threading.Tasks;
using Sandbox;

namespace Garryware;

public class MusicController
{
    private const string BgmLoop = "garryware.bgm.loop";
    private const string NewRoundStinger = "microgame.new";
    private const string RoundWonStinger = "microgame.win";
    private const string RoundLostStinger = "microgame.lose";
    
    public static MusicController Instance { get; private set; }

    private bool bgmLoopStarted;
    private Sound bgmLoopSound;

    public MusicController()
    {
        Instance = this;
    }
    
    public void StartBgmLoop()
    {
        bgmLoopSound.Stop();
        bgmLoopSound = Sound.FromScreen(BgmLoop);
        bgmLoopStarted = true;
    }

    public void StopBgmLoop()
    {
        bgmLoopSound.Stop();
        bgmLoopStarted = false;
    }
    
    public async void PlayNewRoundStinger()
    {
        StopBgmLoop();
        await PlayAndWaitForSound(NewRoundStinger);
        StartBgmLoop();
    }
    
    public async void PlayWonRoundStinger()
    {
        StopBgmLoop();
        await PlayAndWaitForSound(RoundWonStinger);
        StartBgmLoop();
    }
    
    public async void PlayLostRoundStinger()
    {
        StopBgmLoop();
        await PlayAndWaitForSound(RoundLostStinger);
        StartBgmLoop();
    }

    private static Task PlayAndWaitForSound(string eventName)
    {
        var sound = Sound.FromScreen(eventName);
        while (!sound.Finished)
        {
            return GameTask.Yield();
        }
        return GameTask.CompletedTask;
    }
    
    // -------------------------------------------------------------------------
    // Console commands for testing

    [ConCmd.Client("bgm_start")]
    private static void DebugStartBgmLoop()
    {
        Instance.StartBgmLoop();
    }
    
    [ConCmd.Client("bgm_stop")]
    private static void DebugStopBgmLoop()
    {
        Instance.StopBgmLoop();
    }
    
    [ConCmd.Client("bgm_new")]
    private static void DebugNewRound()
    {
        Instance.PlayNewRoundStinger();
    }
    
    [ConCmd.Client("bgm_won")]
    private static void DebugWonRound()
    {
        Instance.PlayWonRoundStinger();
    }
    
    [ConCmd.Client("bgm_lost")]
    private static void DebugLostRound()
    {
        Instance.PlayLostRoundStinger();
    }
    
}