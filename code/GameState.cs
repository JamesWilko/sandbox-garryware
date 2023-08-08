using Core.StateMachine;

namespace Garryware;

public enum GameState
{
    [DisabledState] NotRunning = -1,
    [InitialState] WaitingForPlayers = 0,
    [State] StartingSoon = 1 << 0,
    [State] Instructions = 1 << 1,
    [State] Playing = 1 << 2,
    [State] GameOver = 1 << 3,
    [State] WrongMap = 1 << 5,
    
    [State] Dev = 1 << 4,
    
    [CompositeState] GameInProgress = Instructions | Playing
}
