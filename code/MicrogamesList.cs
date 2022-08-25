using System;
using Garryware.Microgames;

namespace Garryware;

public static class MicrogamesList
{
    public static readonly Microgame[] Microgames =
    {
        new BreakCrates(),
        new BreakAllCrates(),
        new DontStopSprinting(),
        new CrateColorMemory(),
        new CrateColorRoulette(),
    };
}
