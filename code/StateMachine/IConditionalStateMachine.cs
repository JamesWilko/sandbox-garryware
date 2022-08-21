
using System;

namespace Core.StateMachine
{
    public interface IConditionalStateMachine
    {
        void Update();
        void MakeConditional<TParentState>(IObservableStateMachine<TParentState> parent, TParentState condition) where TParentState : Enum;
        void MakeUnconditional<TParentState>(IObservableStateMachine<TParentState> parent, TParentState condition) where TParentState : Enum;
    }
}
