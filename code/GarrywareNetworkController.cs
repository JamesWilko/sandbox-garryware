using System.Linq;
using System.Threading.Tasks;
using Garryware.Utilities;
using Sandbox;

namespace Garryware;

public class GarrywareNetworkController : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerStatePrefab { get; set; }
	[Property] public GameObject PlayerPawnPrefab { get; set; }

	protected override async Task OnLoad()
	{
		Validation.PrefabExists(PlayerStatePrefab, nameof(PlayerStatePrefab));
		Validation.PrefabExists(PlayerPawnPrefab, nameof(PlayerPawnPrefab));
		
		if (Scene.IsEditor)
			return;

		if (!Networking.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f);
			Networking.CreateLobby();
		}
	}
	
	public void OnActive(Connection channel)
	{
		Log.Info($"Player '{channel.DisplayName}' ({channel.SteamId}) has joined the game");

		// Setup the player state for this connecting player
		var ps = FindOrCreatePlayerState(channel);
		if (ps.Network.Active)
		{
			ps.Network.AssignOwnership(channel);
		}
		else
		{
			ps.GameObject.NetworkSpawn(channel);
		}
		ps.ClientInitRpc();
		
		// Setup the pawn for this player
		var pawn = CreatePlayerPawn(channel, ps);
		pawn.GameObject.NetworkSpawn(channel);
	}

	private Transform GetSpawnPoint()
	{
		// @todo
		return global::Transform.Zero;
	}
	
	private PlayerState FindOrCreatePlayerState(Connection channel)
	{
		// Check if there is a player state already around for this connection
		var existing = PlayerState.All.FirstOrDefault(x => x.SteamId == channel.SteamId);
		if (existing.IsValid())
			return existing;
		
		// Player state doesn't exist for this one, create one
		var go = PlayerStatePrefab.Clone(Vector3.Zero);
		go.BreakFromPrefab();
		go.Name = $"PlayerState ({channel.DisplayName} :: {channel.SteamId})";
		go.Network.SetOrphanedMode(NetworkOrphaned.ClearOwner);
		
		var ps = go.GetComponent<PlayerState>();
		ps.SteamId = channel.SteamId;
		ps.SteamName = channel.DisplayName;
		return ps;
	}

	private GarrywarePawn CreatePlayerPawn(Connection channel, PlayerState playerState)
	{
		var pawnGo = PlayerPawnPrefab.Clone(GetSpawnPoint());
		pawnGo.BreakFromPrefab();
		pawnGo.Name = $"Pawn ({channel.DisplayName} :: {channel.SteamId})";
		pawnGo.Network.SetOrphanedMode(NetworkOrphaned.Destroy);
		
		var pawn = pawnGo.GetComponent<GarrywarePawn>();
		return pawn;
	}

}
