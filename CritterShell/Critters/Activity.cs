using System;

namespace CritterShell.Critters
{
    [Flags]
    internal enum Activity
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
