using System;
using NodaTime;
using NodaTime.Extensions;

namespace Callouts
{
    internal static class Extentions
    {
        // TODO: Could probably combine these by passing in current and destination timezone or something
        public static DateTime UtcToCst(this DateTime dateTime)
        {
            // Convert the time to a NodaTime Instant
            // Convert it to a NodaTime ZonedDateTime in CST
            // Convert it back to a date time.
            // This loses the actual timezone, but that is okay because this is just used to display
            return dateTime.ToInstant().InZone(DateTimeZoneProviders.Tzdb["US/Central"]).ToDateTimeUnspecified();
        }

        public static DateTime CstToUtc(this DateTime datetime)
        {
            DateTimeZone cstTz = DateTimeZoneProviders.Tzdb["US/Central"];
            LocalDateTime localTime = LocalDateTime.FromDateTime(datetime);
            ZonedDateTime cstTime = cstTz.AtStrictly(localTime);
            return cstTime.ToDateTimeUtc();
        }
    }
}