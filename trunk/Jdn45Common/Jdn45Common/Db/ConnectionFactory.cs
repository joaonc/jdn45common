using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Sql Connection Factory.
    /// Manages connections so all you need to do is set it once and call it afterwards anywhere in your application, since this is a static class.
    /// 
    /// Not multi-thread safe.
    /// 
    /// Usage: 1) SetConnection(...)
    ///        2) GetConnection() to get the connection and use it.
    /// 
    /// Good references:
    ///        http://www.sqlstrings.com
    /// </summary>
    public static class ConnectionFactory
    {
        private static string DefaultConnectionName = string.Empty;

        private static Dictionary<string, DbConnection> dbConnectionsDict;
        private static Dictionary<string, ConnectionParameters> connectionParametersDict;

        static ConnectionFactory()
        {
            dbConnectionsDict = new Dictionary<string, DbConnection>();
            connectionParametersDict = new Dictionary<string,ConnectionParameters>();
        }

        public static void SetConnection(string server, string database, string user, string password, DbFamily dbFamily)
        {
            SetConnection(server, database, user, password, dbFamily, DefaultConnectionName);
        }

        public static void SetConnection(string server, string database, string user, string password, DbFamily dbFamily, string connectionName)
        {
            ConnectionParameters parameters = new ConnectionParameters(server, database, user, password, dbFamily);
            parameters.Name = connectionName;
            SetConnection(parameters);
        }

        public static void SetConnection(ConnectionParameters connectionParameters)
        {
            // When setting the connection, the default connection name is set if necessary
            // Can only be set once.
            // This is to allow an application that doesn't care about the connection name (only uses one connection at a time)
            // to work well.
            if (string.IsNullOrEmpty(DefaultConnectionName))
            {
                if (string.IsNullOrEmpty(connectionParameters.Name))
                {
                    DefaultConnectionName = "Default_Connection";
                }
                else
                {
                    DefaultConnectionName = connectionParameters.Name;
                }
            }

            if (string.IsNullOrEmpty(connectionParameters.Name))
            {
                connectionParameters.Name = DefaultConnectionName;
            }

            bool connectionExists = false;
            if (connectionParametersDict.ContainsKey(connectionParameters.Name))
            {
                // Connection with that name already exists
                // Check if parameters are the same and dispose current if so
                if (connectionParameters.Equals(connectionParametersDict[connectionParameters.Name]))
                {
                    connectionExists = true;
                }
                else
                {
                    connectionParametersDict.Remove(connectionParameters.Name);
                }
            }

            if (!connectionExists)
            {
                connectionParametersDict.Add(connectionParameters.Name, connectionParameters);
                CreateConnection(connectionParameters.Name);
            }
        }

        private static void CreateConnection(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnectionName;
            }

            ConnectionParameters connectionParameters = connectionParametersDict[connectionName];
            DbConnection connection = null;
            if (dbConnectionsDict.ContainsKey(connectionName))
            {
                connection = dbConnectionsDict[connectionName];
            }

            if (connection != null &&
                (connection.State == System.Data.ConnectionState.Open || connection.State == System.Data.ConnectionState.Broken))
            {
                connection.Close();
                connection.Dispose();
            }

            string connectionString = connectionParameters.GetOleDbConnectionString();
            switch (connectionParameters.DbFamily)
            {
                case DbFamily.SqlServer:
                    connection = new SqlConnection(
                        string.Format("server={0};database={1};user id={2};password={3}",
                            connectionParameters.Server, connectionParameters.Database, connectionParameters.User, connectionParameters.Password));
                    break;
                case DbFamily.Access:
                    connection = new OleDbConnection(connectionParameters.GetOleDbConnectionString());
                    break;
                default:
                    throw new Exception("Connection type not implemented: " + connectionParameters.DbFamily.ToString());
            }

            connection.Open();

            if (dbConnectionsDict.ContainsKey(connectionName))
            {
                dbConnectionsDict[connectionName] = connection;
            }
            else
            {
                dbConnectionsDict.Add(connectionName, connection);
            }
        }

        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static DbConnection GetConnection()
        {
            return GetConnection(string.Empty);
        }

        public static DbConnection GetConnection(ConnectionParameters connectionParameters)
        {
            return GetConnection(connectionParameters.Name);
        }

        public static DbConnection GetConnection(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnectionName;
            }

            if (!dbConnectionsDict.ContainsKey(connectionName))
            {
                throw new Exception("Database connection needs to be set: " + connectionName);
            }

            DbConnection connection = dbConnectionsDict[connectionName];
            if (connection.State == System.Data.ConnectionState.Broken)
            {
                connection.Close();
            }

            if (connection.State == System.Data.ConnectionState.Closed)
            {
                CreateConnection(connectionName);
            }

            return connection;
        }

        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static void RemoveConnection()
        {
            RemoveConnection(string.Empty);
        }

        public static void RemoveConnection(ConnectionParameters connectionParameters)
        {
            RemoveConnection(connectionParameters.Name);
        }

        public static void RemoveConnection(string connectionName)
        {
            if (connectionParametersDict.ContainsKey(connectionName))
            {
                connectionParametersDict.Remove(connectionName);
            }

            if (dbConnectionsDict.ContainsKey(connectionName))
            {
                DbConnection connection = dbConnectionsDict[connectionName];
                if (connection != null &&
                    (connection.State == System.Data.ConnectionState.Open || connection.State == System.Data.ConnectionState.Broken))
                {
                    connection.Close();
                    connection.Dispose();
                }

                dbConnectionsDict.Remove(connectionName);
            }
        }

        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static DbCommand GetCommand(string cmdText)
        {
            return GetCommand(cmdText, string.Empty);
        }

        public static DbCommand GetCommand(string cmdText, ConnectionParameters connectionParameters)
        {
            return GetCommand(cmdText, connectionParameters.Name);
        }

        public static DbCommand GetCommand(string cmdText, string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnectionName;
            }

            DbConnection connection = GetConnection(connectionName);
            ConnectionParameters connectionParameters = connectionParametersDict[connectionName];

            DbCommand command;
            switch (connectionParameters.DbFamily)
            {
                case DbFamily.SqlServer:
                    command = new SqlCommand(cmdText, (SqlConnection)connection);
                    break;
                case DbFamily.Access:
                    command = new OleDbCommand(cmdText, (OleDbConnection)connection);
                    break;
                default:
                    throw new Exception("Command needs to be implemented for database of family " + connectionParameters.DbFamily.ToString());
            }

            return command;
        }

        /// <summary>
        /// Returns a string with information about the default DB connection.
        /// The string will be empty if there's no connection.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static string GetConnectionInfo()
        {
            return GetConnectionInfo("");
        }

        /// <summary>
        /// Returns a string with information about the DB connection.
        /// The string will be empty if there's no connection.
        /// </summary>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public static string GetConnectionInfo(ConnectionParameters connectionParameters)
        {
            return GetConnectionInfo(connectionParameters.Name);
        }

        /// <summary>
        /// Returns a string with information about the DB connection.
        /// The string will be empty if there's no connection.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static string GetConnectionInfo(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnectionName;
            }

            if (!dbConnectionsDict.ContainsKey(connectionName))
            {
                return "";
            }

            DbConnection connection = dbConnectionsDict[connectionName];
            return string.Format("Server: {0}, Database: {1}", connection.DataSource, connection.Database);
        }

        /// <summary>
        /// Gets the Conneciton Parameters by the connection name.
        /// Returns null if not found.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static ConnectionParameters GetParameters(string connectionName)
        {
            if (!connectionParametersDict.ContainsKey(connectionName))
            {
                return null;
            }

            return connectionParametersDict[connectionName];
        }
    }
}
