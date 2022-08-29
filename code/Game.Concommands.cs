using System;
using Garryware.Entities;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame
{
    
    [ConCmd.Server("gw_dev")]
    public static void EnableDevMode(string param)
    {
        Current?.RequestTransition(GameState.Dev);
        
        foreach (var client in To.Everyone)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                BaseCarriable weapon;
                if (param == "pistol")
                {
                    weapon = new Pistol();
                }
                else if (param == "gravgun")
                {
                    weapon = new GravGun();
                    var ent = new BreakableProp
                    {
                        Position = player.EyePosition,
                        Model = CommonEntities.Crate,
                        CanGib = false
                    };
                }
                else
                {
                    Log.Error($"Invalid weapon type {param}");
                    return;
                }
                player.Inventory.Add(weapon, true);
            }
        }
    }
    
    [ConCmd.Server("gw_randomize_points")]
    public static void RandomizePoints()
    {
        foreach (var client in Client.All)
        {
            client.SetInt(Garryware.Tags.Points, Rand.Int(2, 50));
        }
    }
    
    [ConCmd.Server("gw_force_ready")]
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
    
    [ConCmd.Server("gw_random_controls")]
    public static void RandomizeControls()
    {
        Current.AvailableActions = PlayerAction.None;
        foreach (var control in Enum.GetValues<PlayerAction>())
        {
            if(Rand.Float() > 0.5f)
                Current.AvailableActions |= control;
        }
    }
    
    [ConCmd.Server("gw_hide_controls")]
    public static void HideControls()
    {
        Current.AvailableActions = PlayerAction.None;
    }
    
}
