using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Bitwise options for DB queries.
    /// </summary>
    public enum DbQueryOptions
    {
        /// <summary>
        /// No query options specified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Query uses Sql LIKE wildcard to the left of the value, ex. '%value'
        /// </summary>
        LikeLeft = 1,
        /// <summary>
        /// Query uses Sql LIKE wildcard to the left of the value, ex. 'value%'
        /// </summary>
        LikeRight = 2,
        /// <summary>
        /// Query uses Sql LIKE wildcard both to the left and right of the value, ex. '%value%'.
        /// This is a composit value.
        /// </summary>
        LikeBoth = 3,
        /// <summary>
        /// Query uses diacritics insensitive collation.
        /// Assumes Collation to be Latin1_General_CI_AS.
        /// </summary>
        DiacriticsInsensitive = 4,
        /// <summary>
        /// Query uses diacritics sensitive collation (default).
        /// Assumes Collation to be Latin1_General_CI_AS. This collation is already diacritics sensitive
        /// and this option forces it to appear in the query.
        /// </summary>
        DiacriticsSensitive = 8,
        /// <summary>
        /// Query uses diacritics insensitive collation (default).
        /// Assumes Collation to be Latin1_General_CI_AS. This collation is already case insensitive
        /// and this option forces it to appear in the query.
        /// </summary>
        CaseInsensitive = 16,
        /// <summary>
        /// Query uses case sensitive collation.
        /// Assumes Collation to be Latin1_General_CI_AS.
        /// </summary>
        CaseSensitive = 32,
        /// <summary>
        /// Do not place apostrophe ' in the begining and end of the right part of the WHERE statement.
        /// Use especially when the right part of the WHERE is a function or another SQL statement.
        /// </summary>
        SkipApostrophe = 64
    }
}
