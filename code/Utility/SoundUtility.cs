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
    public static void PlayNewRound()
    {
        Sound.FromScreen("microgame.new");
    }
    
    [ClientRpc]
    public static void PlayWinRound()
    {
        Sound.FromScreen("microgame.win");
    }
    
    [ClientRpc]
    public static void PlayLoseRound()
    {
        Sound.FromScreen("microgame.lose");
    }

    [ClientRpc]
    public static void PlayEveryoneWon()
    {
        Sound.FromScreen("microgame.win.everyone");
    }

    [ClientRpc]
    public static void PlayEveryoneLost()
    {
        Sound.FromScreen("microgame.lose.everyone");
    }

    public static void PlayPlayerLockedInWin(Entity playerEntity)
    {
        Sound.FromEntity($"microgame.lock-in-win.{(playerEntity.IsLocalPawn ? "local" : "other")}", playerEntity);
    }
    
    public static void PlayPlayerLockedInLose(Entity playerEntity)
    {
        Sound.FromEntity($"microgame.lock-in-lose.{(playerEntity.IsLocalPawn ? "local" : "other")}", playerEntity);
    }
    
}
