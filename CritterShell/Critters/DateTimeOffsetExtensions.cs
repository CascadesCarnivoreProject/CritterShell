using System;

namespace CritterShell.Critters
{
    internal static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset SetOffset(this DateTimeOffset dateTime, TimeSpan offset)
        {
            return new DateTimeOffset(dateTime.DateTime.AsUnspecifed(), offset);
        }
    }
}
