using System;

namespace CritterShell.Critters
{
    [Flags]
    public enum Activity
    {
        Unknown,
        Begging,
        Courtship,
        Feeding,
        Foraging,
        Grooming,
        Moving,
        NotApplicable,
        Other,
        PairFormation,
        Resting
    }
}
