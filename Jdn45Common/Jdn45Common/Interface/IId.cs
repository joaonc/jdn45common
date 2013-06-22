using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Interface
{
    /// <summary>
    /// Has an Id that is defined as a unique int.
    /// </summary>
    public interface IId
    {
        int Id
        {
            get;
            set;
        }
    }
}
