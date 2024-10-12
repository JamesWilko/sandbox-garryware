using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Garryware;

public class PlayerState : Component
{
	/// <summary>
	/// A quick way to get the local player state on whichever client we access it from.
	/// Null on dedicated server.
	/// </summary>
	public static PlayerState Local { get; private set; }

	/// <summary>
	/// A list of all player states.
	/// </summary>
	public static List<PlayerState> All { get; private set; } = new();
	
	/// <summary>
	/// The Steam ID of whoever owns this player state.
	/// We want this in case people leave to reconnect them to their player state.
	/// </summary>
	[HostSync, Property, ReadOnly] public ulong SteamId { get; set; }
	
	/// <summary>
	/// The name of whoever owns this player state.
	/// We want this stored in case people leave so we can continue to display their name.
	/// </summary>
	[HostSync, Property, ReadOnly] public string SteamName { get; set; }

	/// <summary>
	/// Is this player a bot?
	/// Used for testing.
	/// </summary>
	[HostSync, Property, ReadOnly] public bool IsBot { get; set; } // @todo:
	
	/// <summary>
	/// The name we want to display for this player.
	/// </summary>
	public string DisplayName => IsBot ? $"[Bot] {SteamName}" : SteamName;
	
	[Authority]
	public void ClientInitRpc()
	{
		if (IsBot)
			return;

		Local = this;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		
		All.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		All.Remove(this);
	}
}
