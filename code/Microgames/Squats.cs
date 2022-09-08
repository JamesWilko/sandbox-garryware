using System.Collections.Generic;
using Sandbox;

namespace Garryware.Microgames;

public class Squats : Microgame
{
    class SquatData
    {
        public int Total;
        public bool IsDucking;
        public bool HasSquatted;
        public TimeSince TimeSinceSwitchingStance;
    }
    
    private int target;
    private readonly Dictionary<GarrywarePlayer, SquatData> playerData = new();

    private const float DownTime = 0.25f;
    private const float UpTime = 0.1f;

    public Squats()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.Crouch;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        GameLength = 10;
    }
    
    public override void Setup()
    {
        target = Rand.Int(4, 10);
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions(string.Format("Squat at least {0} times!", target)); // @localization
    }

    public override void Tick()
    {
        base.Tick();

        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                // Make sure player has their data
                if (!playerData.ContainsKey(player))
                {
                    playerData.Add(player, new SquatData());
                }

                var squatData = playerData[player];
                
                // Check if we started or stopped crouching
                if (player.IsDucking && !squatData.IsDucking)
                {
                    squatData.IsDucking = true;
                    squatData.TimeSinceSwitchingStance = 0.0f;
                }
                else if(!player.IsDucking && squatData.IsDucking)
                {
                    squatData.IsDucking = false;
                    squatData.TimeSinceSwitchingStance = 0.0f;
                }
                
                // Check if enough time has passed to consider it a successful squat
                if (squatData.IsDucking
                    && !squatData.HasSquatted
                    && squatData.TimeSinceSwitchingStance >= DownTime)
                {
                    squatData.HasSquatted = true;
                    SoundUtility.PlaySmallTargetHit(To.Single(player));
                }

                if (squatData.HasSquatted
                    && !squatData.IsDucking
                    && squatData.TimeSinceSwitchingStance >= UpTime)
                {
                    squatData.HasSquatted = false;
                    squatData.Total++;
                    
                    if (squatData.Total >= target)
                    {
                        player.FlagAsRoundWinner();
                    }
                    else
                    {
                        SoundUtility.PlayTargetHit(To.Single(player));
                    }
                }
            }
        }
        
    }

    public override void Finish()
    {
    }

    public override void Cleanup()
    {
        playerData.Clear();
    }
    
}
