using System;
using System.Collections.Generic;
using Core.StateMachine;
using Core.StateMachine.Extensions;
using Sandbox;

namespace Garryware;

public partial class GarrywareGame : IObservableStateMachine<GameState>
{
    private struct TransitionRequest
    {
        public GameState State;
        public TransitionSuccess Success;
        public TransitionError Error;
        public TransitionArgs<GameState> Args;

        public TransitionRequest(GameState state, TransitionSuccess success, TransitionError error, TransitionArgs<GameState> args)
        {
            State = state;
            Success = success;
            Error = error;
            Args = args;
        }
    }

    [Net] private GameState CurrentState { get; set; }
    private TransitionArgs<GameState> currentTransitionArgs;

    private static readonly GameState InitialState = StateAttributeUtils.GetInitialState<GameState>();
    private static readonly GameState DisabledState = StateAttributeUtils.GetDisabledState<GameState>();

    private TransitionRequest? delayedTransitionRequest;
    private bool forceDelayRequests;

    private List<TransitionObserver<GameState>> transitionObservers = new List<TransitionObserver<GameState>>();
    private Dictionary<GameState, List<TransitionObserver<GameState>>> enterStateObservers = new Dictionary<GameState, List<TransitionObserver<GameState>>>();
    private Dictionary<GameState, List<TransitionObserver<GameState>>> exitStateObservers = new Dictionary<GameState, List<TransitionObserver<GameState>>>();

    private List<TransitionController<GameState>> transitionControllers = new List<TransitionController<GameState>>();
    private Dictionary<GameState, List<TransitionController<GameState>>> enterStateControllers = new Dictionary<GameState, List<TransitionController<GameState>>>();
    private Dictionary<GameState, List<TransitionController<GameState>>> exitStateControllers = new Dictionary<GameState, List<TransitionController<GameState>>>();

    private List<IConditionalStateMachine> children = new List<IConditionalStateMachine>();

    private bool IsSubStateOrEqual(GameState subState, GameState state)
    {
        return Convert.ToInt32(subState) != 0
               && !state.Equals(DisabledState)
               && state.HasFlag(subState)
               || subState.Equals(state);
    }

    public GameState State => CurrentState;
    public bool IsDisposed { get; private set; }
    public bool IsEnabled => !CurrentState.Equals(DisabledState);
    public bool IsInState(GameState state) => IsSubStateOrEqual(CurrentState, state);

    public bool IsDelayedState(GameState state)
    {
        var delayedRequest = delayedTransitionRequest.GetValueOrDefault();
        GameState delayedState = delayedRequest.State;
        return IsSubStateOrEqual(delayedState, state);
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        Disable();

        IsDisposed = true;

        delayedTransitionRequest = null;
        transitionObservers = null;
        enterStateObservers = null;
        exitStateObservers = null;
        transitionControllers = null;
        enterStateControllers = null;
        exitStateControllers = null;
        children = null;
    }

    public void Enable(object userData = null)
    {
        if (IsDisposed) throw new ObjectDisposedException($"Trying to enable a disposed state machine {this}");

        if (!IsEnabled)
        {
            OnSuccess(InitialState, null, new TransitionArgs<GameState>()
            {
                From = CurrentState,
                To = InitialState,
                UserData = userData
            });
        }
    }

    public void Disable()
    {
        if (!IsEnabled) return;

        OnCancelled(delayedTransitionRequest?.Error);
        OnSuccess(DisabledState, null, new TransitionArgs<GameState>()
        {
            From = CurrentState,
            To = DisabledState
        });
    }

    public void RequestTransition(GameState requestedState, TransitionSuccess success = null, TransitionError error = null, object userData = null)
    {
        if (!IsEnabled)
        {
            Log.Info($"StateMachine<{typeof(GameState)}> blocked going to {requestedState} (not enabled)");
            OnBlockedDisabled(error);
            return;
        }

        if (delayedTransitionRequest.HasValue)
        {
            Log.Info($"StateMachine<{typeof(GameState)}> cancelled going to {delayedTransitionRequest.GetValueOrDefault().State} because a new request to {requestedState} arrived");
            OnCancelled(delayedTransitionRequest?.Error);
            delayedTransitionRequest = null;
        }

        InternalRequestTransition(requestedState, success, error, new TransitionArgs<GameState>()
        {
            From = CurrentState,
            To = requestedState,
            UserData = userData
        });
    }

    [Event.Tick.Server, Event.Tick.Client]
    public void Update()
    {
        if (delayedTransitionRequest.HasValue)
        {
            var request = delayedTransitionRequest.GetValueOrDefault();
            delayedTransitionRequest = null;
            InternalRequestTransition(request.State, request.Success, request.Error, request.Args);
        }

        foreach (var child in children)
        {
            child.Update();
        }
    }

    public void MakeConditional<TParentState>(IObservableStateMachine<TParentState> parent, TParentState condition) where TParentState : Enum
    {
        parent.AddEnterStateObserver(condition, ParentConditionalStateEntered);
        parent.AddExitStateObserver(condition, ParentConditionalStateExited);

        if (parent.IsInState(condition))
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    public void MakeUnconditional<TParentState>(IObservableStateMachine<TParentState> parent, TParentState condition) where TParentState : Enum
    {
        parent.RemoveEnterStateObserver(condition, ParentConditionalStateEntered);
        parent.RemoveExitStateObserver(condition, ParentConditionalStateExited);

        if (!IsEnabled)
        {
            Enable();
        }
    }

    public void AddConditionalChild(GameState condition, IConditionalStateMachine child)
    {
        child.MakeConditional(this, condition);
        children.Add(child);
    }

    public void RemoveConditionalChild(GameState condition, IConditionalStateMachine child)
    {
        child.MakeUnconditional(this, condition);
        children.Remove(child);
    }

    public void AddTransitionObserver(TransitionObserver<GameState> observer)
    {
        transitionObservers.Add(observer);
    }

    public void RemoveTransitionObserver(TransitionObserver<GameState> observer)
    {
        transitionObservers.Remove(observer);
    }

    public void AddTransitionController(TransitionController<GameState> controller)
    {
        transitionControllers.Add(controller);
    }

    public void RemoveTransitionController(TransitionController<GameState> controller)
    {
        transitionControllers.Remove(controller);
    }

    public void AddEnterStateObserver(GameState state, TransitionObserver<GameState> observer, bool triggerImmediately = false)
    {
        if (!enterStateObservers.ContainsKey(state))
        {
            enterStateObservers[state] = new List<TransitionObserver<GameState>>();
        }

        enterStateObservers[state].Add(observer);
        if (triggerImmediately && IsInState(state))
        {
            observer?.Invoke(currentTransitionArgs);
        }
    }

    public void RemoveEnterStateObserver(GameState state, TransitionObserver<GameState> observer)
    {
        if (enterStateObservers.ContainsKey(state))
        {
            enterStateObservers[state].Remove(observer);
        }
    }

    public void AddExitStateObserver(GameState state, TransitionObserver<GameState> observer, bool triggerImmediately = false)
    {
        if (!exitStateObservers.ContainsKey(state))
        {
            exitStateObservers[state] = new List<TransitionObserver<GameState>>();
        }

        exitStateObservers[state].Add(observer);
        if (triggerImmediately && IsInState(state))
        {
            observer?.Invoke(currentTransitionArgs);
        }
    }

    public void RemoveExitStateObserver(GameState state, TransitionObserver<GameState> observer)
    {
        if (exitStateObservers.ContainsKey(state))
        {
            exitStateObservers[state].Remove(observer);
        }
    }

    public void AddEnterStateController(GameState state, TransitionController<GameState> controller)
    {
        if (!enterStateControllers.ContainsKey(state))
        {
            enterStateControllers[state] = new List<TransitionController<GameState>>();
        }

        enterStateControllers[state].Add(controller);
    }

    public void RemoveEnterStateController(GameState state, TransitionController<GameState> controller)
    {
        if (enterStateControllers.ContainsKey(state))
        {
            enterStateControllers[state].Remove(controller);
        }
    }

    public void AddExitStateController(GameState state, TransitionController<GameState> controller)
    {
        if (!exitStateControllers.ContainsKey(state))
        {
            exitStateControllers[state] = new List<TransitionController<GameState>>();
        }

        exitStateControllers[state].Add(controller);
    }

    public void RemoveExitStateController(GameState state, TransitionController<GameState> controller)
    {
        if (exitStateControllers.ContainsKey(state))
        {
            exitStateControllers[state].Remove(controller);
        }
    }

    private void ParentConditionalStateEntered<TParentState>(TransitionArgs<TParentState> transitionArgs) where TParentState : Enum
    {
        Log.Info($"StateMachine<{typeof(GameState)}> is enabled");
        Enable(transitionArgs.UserData);
    }

    private void ParentConditionalStateExited<TParentState>(TransitionArgs<TParentState> transitionArgs) where TParentState : Enum
    {
        Log.Info($"StateMachine<{typeof(GameState)}> is disabled");
        Disable();
    }

    private void OnSuccess(GameState toState, TransitionSuccess success, TransitionArgs<GameState> transitionArgs)
    {
        delayedTransitionRequest = null;

        GameState fromState = CurrentState;
        CurrentState = toState;
        currentTransitionArgs = transitionArgs;

        success?.Invoke();

        forceDelayRequests = true;

        var localTransitionObservers = new List<TransitionObserver<GameState>>(transitionObservers);
        foreach (var observer in localTransitionObservers)
        {
            if (transitionObservers.Contains(observer))
            {
                observer?.Invoke(transitionArgs);
            }
        }

        var localExitStateObservers = new Dictionary<GameState, List<TransitionObserver<GameState>>>(exitStateObservers);
        foreach (var observedState in localExitStateObservers.Keys)
        {
            if (IsSubStateOrEqual(fromState, observedState) && !IsSubStateOrEqual(toState, observedState))
            {
                var localObservers = new List<TransitionObserver<GameState>>(localExitStateObservers[observedState]);
                foreach (var observer in localObservers)
                {
                    if (exitStateObservers[observedState].Contains(observer))
                    {
                        observer?.Invoke(transitionArgs);
                    }
                }
            }
        }

        var localEnterStateObservers = new Dictionary<GameState, List<TransitionObserver<GameState>>>(enterStateObservers);
        foreach (var observedState in localEnterStateObservers.Keys)
        {
            if (IsSubStateOrEqual(toState, observedState) && !IsSubStateOrEqual(fromState, observedState))
            {
                var localObservers = new List<TransitionObserver<GameState>>(localEnterStateObservers[observedState]);
                foreach (var observer in localObservers)
                {
                    if (enterStateObservers[observedState].Contains(observer))
                    {
                        observer?.Invoke(transitionArgs);
                    }
                }
            }
        }

        forceDelayRequests = false;
    }

    private void OnDelayed(GameState requestedState, TransitionSuccess success, TransitionError error, TransitionArgs<GameState> transitionArgs)
    {
        delayedTransitionRequest = new TransitionRequest(requestedState, success, error, transitionArgs);
    }

    private void OnCancelled(TransitionError error)
    {
        delayedTransitionRequest = null;
        error?.Invoke(TransitionErrorReason.Cancelled);
    }

    private void OnBlocked(TransitionError error)
    {
        delayedTransitionRequest = null;
        error?.Invoke(TransitionErrorReason.Blocked);
    }

    private void OnBlockedDisabled(TransitionError error)
    {
        delayedTransitionRequest = null;
        error?.Invoke(TransitionErrorReason.Disabled);
    }

    private TransitionResponse GetHighestResponse(TransitionResponse left, TransitionResponse right)
    {
        return (int)left < (int)right ? right : left;
    }

    private void InternalRequestTransition(GameState requestedState, TransitionSuccess success, TransitionError error, TransitionArgs<GameState> transitionArgs)
    {
        if (requestedState.IsInitialState())
        {
            throw new ArgumentException($"Not allowed to transition back to the initial state {requestedState}");
        }

        if (requestedState.IsDisabledState())
        {
            throw new ArgumentException($"Not allowed to explicitly transition to the disable state {requestedState}");
        }

        if (requestedState.IsCompositeState())
        {
            throw new ArgumentException($"Not allowed to make explicit transition to a composite state {requestedState}");
        }

        if (requestedState.Equals(CurrentState))
        {
            Log.Info($"StateMachine<{typeof(GameState)}> transition ignored: {CurrentState} -> {requestedState}");
            return;
        }

        var response = TransitionResponse.Allow;

        // Check if global transition controllers allow this transition or not 
        foreach (var controller in transitionControllers)
        {
            response = GetHighestResponse(controller(transitionArgs), response);
        }

        // Check if any enter state controllers for this particular transition allow it or not
        foreach (var controlledState in enterStateControllers.Keys)
        {
            if (IsSubStateOrEqual(requestedState, controlledState) && !IsSubStateOrEqual(CurrentState, controlledState))
            {
                foreach (var controller in enterStateControllers[controlledState])
                {
                    response = GetHighestResponse(controller(transitionArgs), response);
                }
            }
        }

        // Check if any exit state controllers for this particular transition allow it or not
        foreach (var controlledState in exitStateControllers.Keys)
        {
            if (!IsSubStateOrEqual(requestedState, controlledState) && IsSubStateOrEqual(CurrentState, controlledState))
            {
                foreach (var controller in exitStateControllers[controlledState])
                {
                    response = GetHighestResponse(controller(transitionArgs), response);
                }
            }
        }

        if (response == TransitionResponse.Block)
        {
            Log.Info($"StateMachine<{typeof(GameState)}> transition blocked: {CurrentState} -> {requestedState}");
            OnBlocked(error);
        }
        else if (response == TransitionResponse.Delay || forceDelayRequests)
        {
            OnDelayed(requestedState, success, error, transitionArgs);
        }
        else
        {
            Log.Info($"StateMachine<{typeof(GameState)}> transition success: {CurrentState} -> {requestedState}");
            OnSuccess(requestedState, success, transitionArgs);
        }
    }
}