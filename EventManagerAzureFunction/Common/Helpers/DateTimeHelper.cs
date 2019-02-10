using System;

namespace Common.Helpers
{
    public static class DateTimeHelper
    {
        public static long ToEpochTimestamp(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }
    }
}
