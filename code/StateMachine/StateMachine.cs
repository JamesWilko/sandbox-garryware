using System;
using System.Collections;
using System.Collections.Generic;
using Core.StateMachine.Extensions;

namespace Core.StateMachine
{

    public enum TransitionResponse
    {
        Allow,
        Delay,
        Block
    }
    
    public enum TransitionErrorReason
    {
        None,
        Cancelled,
        Disabled,
        Blocked
    }

    public class TransitionArgs<TState> where TState : Enum
    {
        public TState From;
        public TState To;
        public object UserData;
    }

    public delegate void TransitionSuccess();
    public delegate void TransitionError(TransitionErrorReason reason);
    public delegate void TransitionObserver<TState>(TransitionArgs<TState> transitionArgs) where TState : Enum;
    public delegate TransitionResponse TransitionController<TState>(TransitionArgs<TState> transitionArgs) where TState : Enum;
    
    public class StateMachine<TState> : IConditionalStateMachine, IObservableStateMachine<TState> where TState : Enum
    {
        private struct TransitionRequest
        {
            public TState State;
            public TransitionSuccess Success;
            public TransitionError Error;
            public TransitionArgs<TState> Args;

            public TransitionRequest(TState state, TransitionSuccess success, TransitionError error, TransitionArgs<TState> args)
            {
                State = state;
                Success = success;
                Error = error;
                Args = args;
            }
        }

        private TState currentState;
        private TransitionArgs<TState> currentTransitionArgs;

        private static readonly TState initialState = StateAttributeUtils.GetInitialState<TState>();
        private static readonly TState disabledState = StateAttributeUtils.GetDisabledState<TState>();

        private TransitionRequest? delayedTransitionRequest;
        private bool forceDelayRequests;

        private List<TransitionObserver<TState>> transitionObservers = new List<TransitionObserver<TState>>();
        private Dictionary<TState, List<TransitionObserver<TState>>> enterStateObservers = new Dictionary<TState, List<TransitionObserver<TState>>>();
        private Dictionary<TState, List<TransitionObserver<TState>>> exitStateObservers = new Dictionary<TState, List<TransitionObserver<TState>>>();
        
        private List<TransitionController<TState>> transitionControllers = new List<TransitionController<TState>>();
        private Dictionary<TState, List<TransitionController<TState>>> enterStateControllers = new Dictionary<TState, List<TransitionController<TState>>>();
        private Dictionary<TState, List<TransitionController<TState>>> exitStateControllers = new Dictionary<TState, List<TransitionController<TState>>>();

        private List<IConditionalStateMachine> children = new List<IConditionalStateMachine>();

        private const string LoggingTag = "StateMachine";

        private bool IsSubStateOrEqual(TState subState, TState state)
        {
            return Convert.ToInt32(subState) != 0
                   && !state.Equals(disabledState)
                   && state.HasFlag(subState)
                   || subState.Equals(state);
        }

        public TState State => currentState;
        public bool IsDisposed { get; private set; }
        public bool IsEnabled => !currentState.Equals(disabledState);
        public bool IsInState(TState state) => IsSubStateOrEqual(currentState, state);

        public bool IsDelayedState(TState state)
        {
            var delayedRequest = delayedTransitionRequest.GetValueOrDefault();
            TState delayedState = delayedRequest.State;
            return IsSubStateOrEqual(delayedState, state);
        }

        public void Dispose()
        {
            if(IsDisposed) return;

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
                OnSuccess(initialState, null, new TransitionArgs<TState>()
                {
                    From = currentState,
                    To = initialState,
                    UserData = userData
                });
            }
        }

        public void Disable()
        {
            if(!IsEnabled) return;
            
            OnCancelled(delayedTransitionRequest?.Error);
            OnSuccess(disabledState, null, new TransitionArgs<TState>()
            {
                From = currentState,
                To = disabledState
            });
        }

        public void RequestTransition(TState requestedState, TransitionSuccess success = null, TransitionError error = null, object userData = null)
        {
            if (!IsEnabled)
            {
                Log.Info($"StateMachine<{typeof(TState)}> blocked going to {requestedState} (not enabled)");
                OnBlockedDisabled(error);
                return;
            }

            if (delayedTransitionRequest.HasValue)
            {
	            Log.Info($"StateMachine<{typeof(TState)}> cancelled going to {delayedTransitionRequest.GetValueOrDefault().State} because a new request to {requestedState} arrived");
                OnCancelled(delayedTransitionRequest?.Error);
                delayedTransitionRequest = null;
            }
            
            InternalRequestTransition(requestedState, success, error, new TransitionArgs<TState>()
            {
                From = currentState,
                To = requestedState,
                UserData = userData
            });
        }

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

        public void AddConditionalChild(TState condition, IConditionalStateMachine child)
        {
            child.MakeConditional(this, condition);
            children.Add(child);
        }
        
        public void RemoveConditionalChild(TState condition, IConditionalStateMachine child)
        {
            child.MakeUnconditional(this, condition);
            children.Remove(child);
        }

        public void AddTransitionObserver(TransitionObserver<TState> observer)
        {
            transitionObservers.Add(observer);
        }
        
        public void RemoveTransitionObserver(TransitionObserver<TState> observer)
        {
            transitionObservers.Remove(observer);
        }

        public void AddTransitionController(TransitionController<TState> controller)
        {
            transitionControllers.Add(controller);
        }
        
        public void RemoveTransitionController(TransitionController<TState> controller)
        {
            transitionControllers.Remove(controller);
        }

        public void AddEnterStateObserver(TState state, TransitionObserver<TState> observer, bool triggerImmediately = false)
        {
            if (!enterStateObservers.ContainsKey(state))
            {
                enterStateObservers[state] = new List<TransitionObserver<TState>>();
            }
            
            enterStateObservers[state].Add(observer);
            if(triggerImmediately && IsInState(state))
            {
                observer?.Invoke(currentTransitionArgs);
            }
        }

        public void RemoveEnterStateObserver(TState state, TransitionObserver<TState> observer)
        {
            if (enterStateObservers.ContainsKey(state))
            {
                enterStateObservers[state].Remove(observer);
            }
        }

        public void AddExitStateObserver(TState state, TransitionObserver<TState> observer, bool triggerImmediately = false)
        {
            if (!exitStateObservers.ContainsKey(state))
            {
                exitStateObservers[state] = new List<TransitionObserver<TState>>();
            }
            
            exitStateObservers[state].Add(observer);
            if(triggerImmediately && IsInState(state))
            {
                observer?.Invoke(currentTransitionArgs);
            }
        }

        public void RemoveExitStateObserver(TState state, TransitionObserver<TState> observer)
        {
            if (exitStateObservers.ContainsKey(state))
            {
                exitStateObservers[state].Remove(observer);
            }
        }

        public void AddEnterStateController(TState state, TransitionController<TState> controller)
        {
            if (!enterStateControllers.ContainsKey(state))
            {
                enterStateControllers[state] = new List<TransitionController<TState>>();
            }
            enterStateControllers[state].Add(controller);
        }
        
        public void RemoveEnterStateController(TState state, TransitionController<TState> controller)
        {
            if (enterStateControllers.ContainsKey(state))
            {
                enterStateControllers[state].Remove(controller);
            }
        }
        
        public void AddExitStateController(TState state, TransitionController<TState> controller)
        {
            if (!exitStateControllers.ContainsKey(state))
            {
                exitStateControllers[state] = new List<TransitionController<TState>>();
            }
            exitStateControllers[state].Add(controller);
        }
        
        public void RemoveExitStateController(TState state, TransitionController<TState> controller)
        {
            if (exitStateControllers.ContainsKey(state))
            {
                exitStateControllers[state].Remove(controller);
            }
        }

        private void ParentConditionalStateEntered<TParentState>(TransitionArgs<TParentState> transitionArgs) where TParentState : Enum
        {
	        Log.Info($"StateMachine<{typeof(TState)}> is enabled");
            Enable(transitionArgs.UserData);
        }
        
        private void ParentConditionalStateExited<TParentState>(TransitionArgs<TParentState> transitionArgs) where TParentState : Enum
        {
            Log.Info($"StateMachine<{typeof(TState)}> is disabled");
            Disable();
        }

        private void OnSuccess(TState toState, TransitionSuccess success, TransitionArgs<TState> transitionArgs)
        {
            delayedTransitionRequest = null;

            TState fromState = currentState;
            currentState = toState;
            currentTransitionArgs = transitionArgs;
            
            success?.Invoke();

            forceDelayRequests = true;

            var localTransitionObservers = new List<TransitionObserver<TState>>(transitionObservers);
            foreach (var observer in localTransitionObservers)
            {
                if (transitionObservers.Contains(observer))
                {
                    observer?.Invoke(transitionArgs);
                }
            }

            var localExitStateObservers = new Dictionary<TState, List<TransitionObserver<TState>>>(exitStateObservers);
            foreach (var observedState in localExitStateObservers.Keys)
            {
                if (IsSubStateOrEqual(fromState, observedState) && !IsSubStateOrEqual(toState, observedState))
                {
                    var localObservers = new List<TransitionObserver<TState>>(localExitStateObservers[observedState]);
                    foreach(var observer in localObservers)
                    {
                        if (exitStateObservers[observedState].Contains(observer))
                        {
                            observer?.Invoke(transitionArgs);
                        }
                    }
                }
            }
            
            var localEnterStateObservers = new Dictionary<TState, List<TransitionObserver<TState>>>(enterStateObservers);
            foreach (var observedState in localEnterStateObservers.Keys)
            {
                if (IsSubStateOrEqual(toState, observedState) && !IsSubStateOrEqual(fromState, observedState))
                {
                    var localObservers = new List<TransitionObserver<TState>>(localEnterStateObservers[observedState]);
                    foreach(var observer in localObservers)
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

        private void OnDelayed(TState requestedState, TransitionSuccess success, TransitionError error, TransitionArgs<TState> transitionArgs)
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

        private void InternalRequestTransition(TState requestedState, TransitionSuccess success, TransitionError error, TransitionArgs<TState> transitionArgs)
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

            if (requestedState.Equals(currentState))
            {
	            Log.Info($"StateMachine<{typeof(TState)}> transition ignored: {currentState} -> {requestedState}");
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
                if (IsSubStateOrEqual(requestedState, controlledState) && !IsSubStateOrEqual(currentState, controlledState))
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
                if (!IsSubStateOrEqual(requestedState, controlledState) && IsSubStateOrEqual(currentState, controlledState))
                {
                    foreach (var controller in exitStateControllers[controlledState])
                    {
                        response = GetHighestResponse(controller(transitionArgs), response);
                    }
                }
            }

            if (response == TransitionResponse.Block)
            {
                Log.Info($"StateMachine<{typeof(TState)}> transition blocked: {currentState} -> {requestedState}");
                OnBlocked(error);
            }
            else if(response == TransitionResponse.Delay || forceDelayRequests)
            {
                 OnDelayed(requestedState, success, error, transitionArgs);
            }
            else
            {
                Log.Info($"StateMachine<{typeof(TState)}> transition success: {currentState} -> {requestedState}");
                OnSuccess(requestedState, success, transitionArgs);
            }
        }

    }

}
