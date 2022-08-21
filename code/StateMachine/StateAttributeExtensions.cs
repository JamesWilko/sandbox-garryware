
using System;

namespace Core.StateMachine.Extensions
{

    public static class EnumAttributeUtils
    {
        public static T GetAttribute<T>(object value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? attributes[0] as T : null;
        }
    }
    
    public static class StateAttributeExtensions
    {
        public static T GetAttribute<T, TState>(this TState value) where T : Attribute
        {
            return EnumAttributeUtils.GetAttribute<T>(value);
        }
        
        public static bool IsState(object value) => EnumAttributeUtils.GetAttribute<StateAttribute>(value) != null;
        public static bool IsState<TState>(this TState value) where TState : Enum => GetAttribute<StateAttribute, TState>(value) != null;
        public static bool IsInitialState<TState>(this TState value) where TState : Enum => GetAttribute<InitialStateAttribute, TState>(value) != null;
        public static bool IsDisabledState<TState>(this TState value) where TState : Enum => GetAttribute<DisabledStateAttribute, TState>(value) != null;
        public static bool IsCompositeState<TState>(this TState value) where TState : Enum => GetAttribute<CompositeStateAttribute, TState>(value) != null;
    }
    
}