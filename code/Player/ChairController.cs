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

        if (player.IsServer && player.Parent is Entities.Chair chair)
        {
            if (Input.Pressed(InputButton.Use))
            {
                chair.RemoveSitter();
                return;
            }
        }

        var aimRotation = Input.Rotation.Clamp(EyeRotation, 90);

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper(player);
        animHelper.WithLookAt(player.EyePosition + aimRotation.Forward * 200.0f, 1.0f, 1.0f, 0.5f);
        animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
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
