using Sandbox;

namespace Garryware;

public static class CommonEntities
{
    public static Model Crate;
    public static Model Balloon;
    public static Model Ball;
    public static Model Target;
    
    public static void Precache()
    {
        Crate = Model.Load("models/citizen_props/crate01.vmdl");
        Balloon = Model.Load("models/citizen_props/balloonregular01.vmdl");
        Ball = Model.Load("models/citizen_props/beachball.vmdl");
    }
}
