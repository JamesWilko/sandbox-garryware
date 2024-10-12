namespace Garryware.StateMachine;

public enum GameState
{
	NotRunning = -1,
	WaitingForPlayers = 0,
	StartingSoon = 1 << 0,
	Tutorial = 1 << 1,
	Playing = 1 << 2,
	GameOver = 1 << 3,
	Dev = 1 << 4,
}
