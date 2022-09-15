using System;
using System.Collections.Generic;
using Sandbox;

namespace Garryware;

public enum AnnouncerVoice
{
    AnnouncerTf2,
    Dos,
}

public static partial class SoundUtility
{
    public static AnnouncerVoice CurrentAnnouncer = AnnouncerVoice.AnnouncerTf2;
    
    private static readonly Dictionary<AnnouncerVoice, string> AnnouncerEvents = new()
    {
        {AnnouncerVoice.AnnouncerTf2, "ann"},
        {AnnouncerVoice.Dos, "dos"}
    };

    private const int maxCountdownSeconds = 5;
	private const float newRoundTime = 2.5f;

	private static Sound? bgmLoopSound;

    static SoundUtility()
    {
        GameEvents.OnPlayerWon += OnPlayerWon;
        GameEvents.OnPlayerLost += OnPlayerLost;
    }
    
    public static void PlayCountdown(int seconds)
    {
        if(seconds > maxCountdownSeconds) return;
        Sound.FromScreen($"microgame.countdown.{AnnouncerEvents[CurrentAnnouncer]}.{seconds}");
    }

    public static void PlayClockTick(int seconds)
    {
        Sound.FromScreen($"microgame.clock.tick.{(seconds % 2 == 0 ? "high" : "low")}");
    }
    
    [ClientRpc]
    public static async void PlayNewRound(float roundTime)
    {
        Sound.FromScreen("microgame.new");
        StopBGM();

        Sound localBgmLoopSound;
		if (roundTime >= 10.0f)
		{
            localBgmLoopSound = Sound.FromScreen("garryware.bgm.loop.long");
		}
		else
		{
            localBgmLoopSound = Sound.FromScreen("garryware.bgm.loop.short");
		}
        bgmLoopSound = localBgmLoopSound;

        // Operate on a local copy in case this function is called again before the delay finishes
        localBgmLoopSound.SetVolume(0.01f);
        await GameTask.DelayRealtimeSeconds(newRoundTime * 0.8f);
        localBgmLoopSound.SetVolume(1.0f);
	}

	[ClientRpc]
    public static void PlayWinRound()
    {
        Sound.FromScreen("microgame.win");
        StopBGM();
    }
    
    [ClientRpc]
    public static void PlayLoseRound()
    {
        Sound.FromScreen("microgame.lose");
        StopBGM();
    }

    [ClientRpc]
    public static void PlayEveryoneWon()
    {
        Sound.FromScreen("microgame.win.everyone");
        StopBGM();
    }

    [ClientRpc]
    public static void PlayEveryoneLost()
    {
        Sound.FromScreen("microgame.lose.everyone");
        StopBGM();
    }

    [ClientRpc]
    public static void PlayTutorial()
    {
        Sound.FromScreen("garryware.tutorial");
        StopBGM();
    }
    
    [ClientRpc]
    public static void PlayGameOver()
    {
        Sound.FromScreen("garryware.gameover");
        StopBGM();
    }

    [ClientRpc]
    public static void PlayTargetHit()
    {
        Sound.FromScreen("microgame.hit");
    }
    
    [ClientRpc]
    public static void PlaySmallTargetHit()
    {
        Sound.FromScreen("microgame.hit.small");
    }
    
    private static void OnPlayerWon(Client player, RoundResult result)
    {
        Sound.FromEntity($"microgame.lock-in-win.{(player.IsOwnedByLocalClient ? "local" : "other")}", player.Pawn);
    }
    
    private static void OnPlayerLost(Client player, RoundResult result)
    {
        Sound.FromEntity($"microgame.lock-in-lose.{(player.IsOwnedByLocalClient ? "local" : "other")}", player.Pawn);
    }

    private static void StopBGM()
    {
        bgmLoopSound?.Stop();
        bgmLoopSound = null;
    }
}
