using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Db
{
    /// <summary>
    /// The different types of database.
    /// Not called DbType to avoid confusions with System.Data.DbType.
    /// </summary>
    public enum DbFamily
    {
        /// <summary>
        /// Microsoft Access.
        /// </summary>
        Access,
        /// <summary>
        /// Microsoft SQL Server.
        /// </summary>
        SqlServer,
        MySql,
        Oracle
    }
}
