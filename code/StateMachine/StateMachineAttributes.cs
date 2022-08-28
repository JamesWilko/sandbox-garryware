
using System;
using System.Linq;

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

}
