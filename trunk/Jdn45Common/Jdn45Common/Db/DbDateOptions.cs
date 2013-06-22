using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Db
{
    public enum DbDateOptions
    {
        /// <summary>
        /// Until the day only (no time)
        /// </summary>
        Short,
        /// <summary>
        /// The whole date, until seconds.
        /// </summary>
        Full,
        /// <summary>
        /// Full date until seconds, but the time is rounded down to the day.
        /// Ex '2011-12-31 12:00:00' becomes '2011-12-31 00:00:00'
        /// </summary>
        FullDayRoundDown,
        /// <summary>
        /// Full date until seconds, but the time is rounded down to the day.
        /// Ex '2011-12-31 12:00:00' becomes '2011-12-31 23:59:59'
        /// </summary>
        FullDayRoundUp
    }
}
