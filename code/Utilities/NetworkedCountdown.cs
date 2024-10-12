using Sandbox;

namespace Garryware.Utilities;

public struct NetworkedCountdown
{
	public bool IsEnabled;
	public TimeUntil TimeUntilExpires;

	public bool IsComplete => IsEnabled && TimeUntilExpires <= 0f;
	
	public static NetworkedCountdown Get(float duration)
	{
		return new NetworkedCountdown() { IsEnabled = true, TimeUntilExpires = duration };
	}

	public static NetworkedCountdown Disable
	{
		get => new() { IsEnabled = false };
	}
	
	public static implicit operator NetworkedCountdown(float duration) => Get(duration);

}
