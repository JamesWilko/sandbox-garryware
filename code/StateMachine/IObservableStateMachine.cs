
using System;

namespace Core.StateMachine
{
    public interface IObservableStateMachine<TState> : IDisposable
        where TState : Enum
    {
        bool IsDisposed { get; }
        bool IsEnabled { get; }
        bool IsInState(TState state);
        void AddConditionalChild(TState condition, IConditionalStateMachine child);
        void RemoveConditionalChild(TState condition, IConditionalStateMachine child);
        void AddTransitionObserver(TransitionObserver<TState> observer);
        void RemoveTransitionObserver(TransitionObserver<TState> observer);
        void AddEnterStateObserver(TState state, TransitionObserver<TState> observer, bool triggerImmediately = false);
        void RemoveEnterStateObserver(TState state, TransitionObserver<TState> observer);
        void AddExitStateObserver(TState state, TransitionObserver<TState> observer, bool triggerImmediately = false);
        void RemoveExitStateObserver(TState state, TransitionObserver<TState> observer);
    }
}