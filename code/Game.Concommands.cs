using System;
using System.Collections.Generic;
using Garryware.Entities;
using Garryware.UI;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame
{
    
    [ConCmd.Admin("gw_dev")]
    public static void EnableDevMode(string param)
    {
        Current?.RequestTransition(GameState.Dev);
        
        foreach (var client in To.Everyone)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                BaseCarriable weapon = null;
                switch (param)
                {
                    case "pistol":
                        weapon = new Pistol();
                        break;
                    case "fists":
                        weapon = new Fists();
                        break;
                    case "launcher":
                        weapon = new LauncherPistol();
                        break;
                    case "rpg":
                        weapon = new RocketLauncher();
                        break;
                    case "ball":
                        weapon = new BallLauncher();
                        break;
                    case "gravgun":
                        weapon = new GravityGun();
                        _ = new BreakableProp
                        {
                            Position = player.EyePosition + player.EyeRotation.Forward * 60.0f,
                            Model = CommonEntities.Crate
                        };
                        break;
                    case "crate":
                        _ = new BreakableProp
                        {
                            Position = Current.CurrentRoom.AboveBoxSpawnsDeck.Next().Position,
                            Model = CommonEntities.Crate,
                            CanGib = false,
                            ShowWorldText = true,
                            WorldText = "69"
                        };
                        break;
                    default:
                        Log.Error($"Invalid weapon type {param}");
                        return;
                }
                
                if(weapon != null)
                    player.Inventory.Add(weapon, true);
            }
        }
    }
    
    [ConCmd.Admin("gw_randomize_points")]
    public static void RandomizePoints()
    {
        var pointsPlacing = new List<int>();
        foreach (var client in Client.All)
        {
            var points = Rand.Int(2, 50);
            client.SetInt(Garryware.Tags.Points, points);
            pointsPlacing.AddUnique(points);
        }
        
        // Sort points out into their points order and assign a place to each player based on their points 
        pointsPlacing.Sort();
        pointsPlacing.Reverse();
        foreach (var client in Client.All)
        {
            int place = pointsPlacing.IndexOf(client.GetInt(Garryware.Tags.Points)) + 1;
            client.SetInt(Garryware.Tags.Place, place);
        }
        
    }
    
    [ConCmd.Admin("gw_randomize_streak")]
    public static void RandomizeStreak()
    {
        foreach (var client in Client.All)
        {
            client.SetInt(Garryware.Tags.Streak, Rand.Int(2, 10));
            client.SetInt(Garryware.Tags.MaxStreak, Rand.Int(10, 15));
        }
    }
    
    [ConCmd.Admin("gw_force_ready")]
    public static void ForceReadyUp()
    {
        foreach (var client in Client.All)
        {
            if (client.GetInt(Garryware.Tags.IsReady) == 0)
            {
                client.SetInt(Garryware.Tags.IsReady, 1);
                break;
            }
        }

        AttemptToStartGame();
    }
    
    [ConCmd.Admin("gw_force_ready_all")]
    public static void ForceReadyUpAll()
    {
        foreach (var client in Client.All)
        {
            if (client.GetInt(Garryware.Tags.IsReady) == 0)
            {
                client.SetInt(Garryware.Tags.IsReady, 1);
            }
        }

        AttemptToStartGame();
    }
    
    [ConCmd.Admin("gw_random_controls")]
    public static void RandomizeControls()
    {
        Current.AvailableActions = PlayerAction.None;
        foreach (var control in Enum.GetValues<PlayerAction>())
        {
            if(Rand.Float() > 0.5f)
                Current.AvailableActions |= control;
        }
    }
    
    [ConCmd.Admin("gw_hide_controls")]
    public static void HideControls()
    {
        Current.AvailableActions = PlayerAction.None;
    }
    
    [ConCmd.Admin("gw_countdown")]
    public static void ShowCountdown(int seconds)
    {
        Current.SetCountdownTimer(seconds);
    }
    
    [ConCmd.Admin("gw_killfeed_success")]
    public static void ShowKillfeedSuccess()
    {
        GameEvents.PlayerLockedInResult(ConsoleSystem.Caller, RoundResult.Won);
    }
    
    [ConCmd.Admin("gw_killfeed_fail")]
    public static void ShowKillfeedFail()
    {
        GameEvents.PlayerLockedInResult(ConsoleSystem.Caller, RoundResult.Lost);
    }

    [ConCmd.Admin("gw_stat_everybody_won")]
    public static void SendStat_EverybodyWon()
    {
        GameEvents.SendIntegerStat(RoundStat.EverybodyWon, 0);
    }

    [ConCmd.Admin("gw_stat_everybody_lost")]
    public static void SendStat_EverybodyLost()
    {
        GameEvents.SendIntegerStat(RoundStat.EverybodyLost, 0);
    }
    
    [ConCmd.Admin("gw_stat_int_test")]
    public static void SendStat_IntegerTest()
    {
        GameEvents.SendIntegerStat(RoundStat.OnlyXPeopleWon, Rand.Int(2, 5));
    }
    
    [ConCmd.Admin("gw_stat_client_test")]
    public static void SendStat_ClientTest()
    {
        GameEvents.SendClientStat(RoundStat.XWasTheFastestToWin, ConsoleSystem.Caller);
    }
    
}
