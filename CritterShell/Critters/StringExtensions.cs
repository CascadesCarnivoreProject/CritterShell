using System;

namespace CritterShell.Critters
{
    internal static class StringExtensions
    {
        public static string ToSite(this string station)
        {
            if (station == null)
            {
                return null;
            }
            return station.Substring(0, Math.Min(4, station.Length));
        }
    }
}
