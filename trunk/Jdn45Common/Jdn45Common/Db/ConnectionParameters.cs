using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Helper class to hold the DB connection parameters.
    /// XML serializable.
    /// </summary>
    [Serializable]
    public class ConnectionParameters
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private string name;
        private string description;
        private DbFamily dbFamily;

        public ConnectionParameters()
        {
            DbFamily = DbFamily.SqlServer;  // Default
        }

        public ConnectionParameters(string server, string database, string user, string password, DbFamily dbFamily)
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
            DbFamily = dbFamily;
        }

        /// <summary>
        /// Name of this connection.
        /// Optional. Not needed to establish the connection.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Description of this connection.
        /// Optional. Not needed to establish the connection.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// The server name (ip:port) or filename.
        /// </summary>
        public string Server
        {
            get { return server; }
            set { server = value; }
        }
        
        public string Database
        {
            get { return database; }
            set { database = value; }
        }

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public DbFamily DbFamily
        {
            get { return dbFamily; }
            set { dbFamily = value; }
        }

        /// <summary>
        /// Gets the OleDbConnection version of the connection string.
        /// This is the more baseline version of the string.
        /// There are more specific connection string that depend on vendor which are are not used by this method.
        /// More details in http://www.sqlstrings.com
        /// </summary>
        /// <returns></returns>
        public string GetOleDbConnectionString()
        {
            string connection = string.Empty;

            switch (dbFamily)
            {
                case DbFamily.SqlServer:
                    connection = string.Format("Provider=SQLOLEDB;Data Source={0};Initial Catalog={1};UserId={2};Password={3};",
                        server, database, user, password);
                    break;

                case DbFamily.Access:
                    // If you get an exception in the lines below, you may need to run the app in 32bit
                    // See the following link for more information
                    // http://stackoverflow.com/questions/1991643/microsoft-jet-oledb-4-0-provider-is-not-registered-on-the-local-machine
                    //
                    // Another option is to install the Microsoft Access Database Engine 2010 Redistributable
                    // and change the connection string
                    // http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=13255

                    connection = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}", server);
                    if (!string.IsNullOrEmpty(password))
                    {
                        connection += string.Format("; Jet OLEDB:Database Password={0}", password);
                    }
                    break;

                case DbFamily.MySql:
                    connection = string.Format("Provider=MySQLProv;Data Source={0};User Id={1}; Password={2};",
                        database, user, password);
                    break;

                case DbFamily.Oracle:
                    connection = string.Format("Provider=MSDAORA;Data Source={0};UserId={1};Password={2};",
                        database, user, password);
                    break;

                default:
                    throw new Exception("Unknown database family: " + dbFamily.ToString());
            }

            return connection;
        }

        /// <summary>
        /// Returns the hash for this type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(server).Append(database).Append(user).Append(password).Append(name).Append(description);

            return sb.ToString().GetHashCode() + (int)dbFamily;
        }

        /// <summary>
        /// Returns true if this object is equal to the one passed in.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ConnectionParameters))
            {
                return false;
            }

            return Equals((ConnectionParameters)obj);
        }

        /// <summary>
        /// Returns true if this object is equal to the one passed in.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(ConnectionParameters obj)
        {
            // Does not compare Name or Description b/c those are just for information
            // and have no effect on the connection

            return
                object.Equals(Server, obj.Server) &&
                object.Equals(Database, obj.Database) &&
                object.Equals(User, obj.User) &&
                object.Equals(Password, obj.Password) &&
                object.Equals(DbFamily, obj.DbFamily);
        }

        /// <summary>
        /// Returns true if none of the database parameters is set.
        /// </summary>
        /// <returns></returns>
        public bool IsBlank()
        {
            return
                string.IsNullOrEmpty(Server) &&
                string.IsNullOrEmpty(Database) &&
                string.IsNullOrEmpty(User) &&
                string.IsNullOrEmpty(Password);
        }
    }
}
