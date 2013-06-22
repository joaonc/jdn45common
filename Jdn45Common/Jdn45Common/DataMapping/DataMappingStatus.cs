using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.DataMapping
{
    public enum DataMappingStatus
    {
        /// <summary>
        /// The mapping is set.
        /// </summary>
        Mapped,
        /// <summary>
        /// The mapping is set to not mapped, ie, it was purposely set to not having a mapping.
        /// </summary>
        NotMapped,
        /// <summary>
        /// The mapping hasn't been set yet.
        /// </summary>
        NotSet
    }
}
