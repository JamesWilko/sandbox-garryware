using System;
using System.Collections;
using System.Collections.Generic;

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
}
