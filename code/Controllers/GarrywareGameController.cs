using Garryware.StateMachine;
using Garryware.Utilities;
using Sandbox;

namespace Garryware;

//
// State control for Garryware, uses an attached State Machine component to control game flow.
//
public class GarrywareGameController : SingletonComponent<GarrywareGameController>
{
	[Property, Group("Starting Soon")] public float ForceReadyUpDelay { get; set; } = 20f;
	[Property, Group("Starting Soon")] public float EveryoneReadyDelay { get; set; } = 3f;
	[Property, Group("Tutorial")] public bool SkipTutorial { get; set; } = false;
	[Property, Group("Starting Soon")] public float GameOverRestartDelay { get; set; } = 30f;
	
	[Sync] public GameState CurrentGameState { get; set; }
	
	[Sync] public int CurrentRoundNumber { get; set; }
	[Sync] public int TotalRounds { get; set; }
	
	[Sync] public int NumberOfWinners { get; set; }
	[Sync] public int NumberOfLosers { get; set; }
	
	/// <summary>
	/// Is there a countdown active and how long does it have left on it?
	/// </summary>
	[HostSync] public NetworkedCountdown Countdown { get; set; }

	/// <summary>
	/// Which actions are important enough for the minigame that they should be told about the inputs?
	/// </summary>
	[HostSync] public PlayerAction AvailableGameActions { get; set; }

	//
	// Control properties for game state machine.
	// These are called from the state machine conditions action graph.
	//
	public bool CanLeaveStartingSoonState => Countdown.IsComplete; // @todo
	private bool StartingSoonCountdownIsComplete => CurrentGameState == GameState.StartingSoon && Countdown.IsComplete;
	public bool StartingSoonTransitionToTutorial => StartingSoonCountdownIsComplete && !SkipTutorial;
	public bool StartingSoonTransitionToGameplay => StartingSoonCountdownIsComplete && SkipTutorial;
	public bool IsTutorialComplete => SkipTutorial; // @todo
	public bool HasFinishedAllRounds => Countdown.IsComplete; // CurrentRoundNumber > TotalRounds; // @todo
	public bool IsReadyToRestartFromBeginning => Countdown.IsComplete; // @todo

	//
	// Control methods for state machine.
	// These are called from the state machine action graph.
	//
	public void OnEnterWaitingForPlayers()
	{
		CurrentGameState = GameState.WaitingForPlayers;
		AvailableGameActions = PlayerAction.ReadyUp;

		// @todo: unready all players
	}
	
	public void OnEnterStartingSoon()
	{
		CurrentGameState = GameState.StartingSoon;
		Countdown = ForceReadyUpDelay;
		
		AvailableGameActions = PlayerAction.Jump | PlayerAction.Crouch;
	}

	public void OnEnterTutorial()
	{
		CurrentGameState = GameState.Tutorial;
		
		// @todo: play tutorial
	}

	public void OnEnterPlayingState()
	{
		CurrentGameState = GameState.Playing;
		
		Countdown = 5;
	}

	public void OnEnterGameOver()
	{
		CurrentGameState = GameState.GameOver;	
		Countdown = GameOverRestartDelay;
	}
	
}
