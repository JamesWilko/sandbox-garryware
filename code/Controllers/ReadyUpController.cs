using System;
using System.Linq;
using Garryware.StateMachine;
using Garryware.Utilities;
using Sandbox;

namespace Garryware;

//
// Controller for making sure the game can start once enough players are ready.
// Used by the StateMachine on the game controller.
//
public class ReadyUpController : SingletonComponent<ReadyUpController>
{
	[Sync] public int NumberOfReadyPlayers { get; set; }
	[Sync] public int NumberRequiredToStart { get; set; }
	
	public bool HaveEnoughPlayersReadiedUp => NumberRequiredToStart > 0 && NumberOfReadyPlayers >= NumberRequiredToStart;

	public bool IsWaitingForPlayers => GarrywareGameController.Instance.CurrentGameState == GameState.WaitingForPlayers && !HaveEnoughPlayersReadiedUp;
	
	/// <summary>
	/// Reset every player to being unready.
	/// </summary>
	public void ResetReadyUpStates()
	{
		foreach (var playerState in PlayerState.All)
		{
			playerState.IsReady = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// @todo: good enough for testing, do this event based not polling
		NumberOfReadyPlayers = PlayerState.All.Count(x => x.IsReady);
		NumberRequiredToStart = Math.Max((int)(PlayerState.All.Count * 0.5f), 1);
	}
}
