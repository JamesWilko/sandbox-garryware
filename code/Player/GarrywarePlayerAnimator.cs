using Sandbox;

namespace Garryware;

public class GarrywarePlayerAnimator : PawnAnimator
{
    Entity lastWeapon;
    public override void Simulate()
    {
        var player = Pawn as Player;
        if (player == null)
            return;

        // where should we be rotated to
        var turnSpeed = 0.02f;
        var idealRotation = Rotation.LookAt(Input.Rotation.Forward.WithZ(0), Vector3.Up);
        Rotation = Rotation.Slerp(Rotation, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed);
        Rotation = Rotation.Clamp(idealRotation, 45.0f, out var shuffle); // lock facing to within 45 degrees of look direction

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper(player);

        animHelper.WithWishVelocity(WishVelocity);
        animHelper.WithVelocity(Velocity);
        animHelper.WithLookAt(player.EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f);
        animHelper.AimAngle = Input.Rotation;
        animHelper.FootShuffle = shuffle;
        animHelper.DuckLevel = MathX.Lerp(animHelper.DuckLevel, HasTag("ducked") ? 1 : 0, Time.Delta * 10.0f);
        animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
        animHelper.IsGrounded = GroundEntity != null;
        animHelper.IsSitting = HasTag("sitting");
        animHelper.IsNoclipping = HasTag("noclip");
        animHelper.IsClimbing = HasTag("climbing");
        animHelper.IsSwimming = player.WaterLevel >= 0.5f;
        animHelper.IsWeaponLowered = false;

        if (HasEvent("jump")) animHelper.TriggerJump();
        if (player.ActiveChild != lastWeapon) animHelper.TriggerDeploy();

        if (player.ActiveChild is BaseCarriable carry)
        {
            carry.SimulateAnimator(animHelper);
        }
        else
        {
            animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
            animHelper.AimBodyWeight = 0.5f;
        }

        lastWeapon = player.ActiveChild;
    }
}
