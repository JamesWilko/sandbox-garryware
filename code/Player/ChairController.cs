using Sandbox;

namespace Garryware;

public partial class ChairController : BasePlayerController
{
    public override void Simulate()
    {
        if (!(Pawn is GarrywarePlayer))
        {
            return;
        }

        var player = Pawn as GarrywarePlayer;
        if (Game.IsServer && player.Parent is Entities.Chair chair)
        {
            if (Input.Pressed("use"))
            {
                chair.RemoveSitter();
                return;
            }
        }

        var aimRotation = Input.AnalogLook.ToRotation().Clamp(EyeRotation, 90);

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper(player);
        animHelper.WithLookAt(player.EyePosition + aimRotation.Forward * 200.0f, 1.0f, 1.0f, 0.5f);
        animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
        animHelper.IsGrounded = true;
        animHelper.IsSitting = true;
        animHelper.IsNoclipping = false;
        animHelper.IsClimbing = false;
        player.SetAnimParameter("sit", 1);

        if (player.ActiveChild is BaseCarriable carry)
        {
            //carry.SimulateAnimator( null );
        }
        else
        {
            animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
        }
    }
}
