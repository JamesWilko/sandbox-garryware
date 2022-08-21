
using System;
using System.Linq;
using Core.StateMachine.Extensions;

namespace Core.StateMachine
{
    
    public class StateAttribute : Attribute
    {
    }

    public class InitialStateAttribute : Attribute
    {
    }
    
    public class DisabledStateAttribute : Attribute
    {
    }
    
    public class CompositeStateAttribute : Attribute
    {
    }

    public static class StateAttributeUtils
    {
        public static TState GetInitialState<TState>() where TState : Enum
        {
            var enumValues = (TState[]) Enum.GetValues(typeof(TState));
            return enumValues.FirstOrDefault(state => state.IsInitialState());
        }
        
        public static TState GetDisabledState<TState>() where TState : Enum
        {
            var enumValues = (TState[]) Enum.GetValues(typeof(TState));
            return enumValues.FirstOrDefault(state => state.IsDisabledState());
        }
    }

}
