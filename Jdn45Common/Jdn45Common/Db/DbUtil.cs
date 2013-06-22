using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Helper class with static methods to execute SQL queries with the connection managed by ConnectionFactory.
    /// </summary>
    public static class DbUtil
    {
        /// <summary>
        /// Runs a SQL Query that only does select statements.
        /// No update, insert or stored procedures.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static DbDataReader RunQuery(string sql)
        {
            return RunQuery(sql, string.Empty);
        }

        /// <summary>
        /// Runs a SQL Query that only does select statements.
        /// No update, insert or stored procedures.
        /// </summary>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public static DbDataReader RunQuery(string sql, ConnectionParameters connectionParameters)
        {
            return RunQuery(sql, connectionParameters.Name);
        }

        /// <summary>
        /// Runs a SQL Query that only does select statements.
        /// No update, insert or stored procedures.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static DbDataReader RunQuery(string sql, string connectionName)
        {
            return ConnectionFactory.GetCommand(sql, connectionName).ExecuteReader();
        }

        /// <summary>
        /// Runs a SQL Stored Procedure that can take parameters.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>The return result of the Stored Procedure.</returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static DbDataReader RunStoredProcedure(string sp, params KeyValuePair<string, object>[] parameters)
        {
            return RunStoredProcedure(sp, null, parameters);
        }

        /// <summary>
        /// Runs a SQL Stored Procedure that can take parameters.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="connectionParameters"></param>
        /// <param name="parameters"></param>
        /// <returns>The return result of the Stored Procedure.</returns>
        public static DbDataReader RunStoredProcedure(string sp, ConnectionParameters connectionParameters, params KeyValuePair<string, object>[] parameters)
        {
            return RunStoredProcedure(sp, connectionParameters, (IEnumerable<KeyValuePair<string, object>>)parameters);
        }

        /// <summary>
        /// Runs a SQL Stored Procedure that can take parameters.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="connectionParameters"></param>
        /// <param name="parameters">Enumerable of KV pairs (can be a Dictionary)</param>
        /// <returns>The return result of the Stored Procedure.</returns>
        public static DbDataReader RunStoredProcedure(string sp, ConnectionParameters connectionParameters, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            DbCommand command = ConnectionFactory.GetCommand(sp, connectionParameters);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                command.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
            }

            return command.ExecuteReader();
        }

        /// <summary>
        /// Runs a SQL Stored Procedure that can take parameters and returns the first column of the first row in the result set returned by the Stored Procedure.
        /// All other columns and rows are ignored.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>The value of the first column of the first row returned by the Stored Procedure executed.</returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static object RunStoredProcedureScalar(string sp, params KeyValuePair<string, object>[] parameters)
        {
            return RunStoredProcedureScalar(sp, null, parameters);
        }

        /// <summary>
        /// Runs a SQL Stored Procedure that can take parameters and returns the first column of the first row in the result set returned by the Stored Procedure.
        /// All other columns and rows are ignored.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="connectionParameters"></param>
        /// <param name="parameters"></param>
        /// <returns>The value of the first column of the first row returned by the Stored Procedure executed.</returns>
        public static object RunStoredProcedureScalar(string sp, ConnectionParameters connectionParameters, params KeyValuePair<string, object>[] parameters)
        {
            DbCommand command = ConnectionFactory.GetCommand(sp, connectionParameters);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                command.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
            }

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Runs the SQL script (may include Update, Insert, Create, etc.) and returns the number of rows affected.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static int RunScript(string script)
        {
            return RunScript(script, string.Empty);
        }

        /// <summary>
        /// Runs the SQL script (may include Update, Insert, Create, etc.) and returns the number of rows affected.
        /// </summary>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public static int RunScript(string script, ConnectionParameters connectionParameters)
        {
            return RunScript(script, connectionParameters.Name);
        }

        /// <summary>
        /// Runs the SQL script (may include Update, Insert, Create, etc.) and returns the number of rows affected.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static int RunScript(string script, string connectionName)
        {
            return ConnectionFactory.GetCommand(script, connectionName).ExecuteNonQuery();
        }

        /// <summary>
        /// Gets a DataTable object from the data reader's contents.
        /// The DbDataReader object is disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static DataTable GetDataTableFromDataReader(IDataReader dr)
        {
            return GetDataTableFromDataReader(dr, null);
        }

        /// <summary>
        /// Gets a DataTable object from the data reader's contents.
        /// The DbDataReader object is disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="tableName">The table name. Can be null for default.</param>
        /// <returns></returns>
        public static DataTable GetDataTableFromDataReader(IDataReader dr, string tableName)
        {
            DataTable dt = new DataTable();
            if (tableName != null)
            {
                dt.TableName = tableName;
            }

            dt.Load(dr);
            dr.Close();
            dr.Dispose();

            return dt;
        }

        /// <summary>
        /// Converts a DbDataReader object with one row to a dictionary.
        /// Dict Keys are DbDataReader Columns and Values is the Row.
        /// Will throw if there is more or less than one result.
        /// Note that DbDataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetDictFromDataReader(DbDataReader dr)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                if (!dr.HasRows)
                {
                    throw new Exception("DataReader has no rows. Needs to have one.");
                }

                dr.Read();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dict.Add(dr.GetName(i), dr[i]);
                }

                if (dr.Read())
                {
                    throw new Exception("DataReader has more than one row. Needs to have only one.");
                }
            }
            finally
            {
                if (dr != null  && !dr.IsClosed)
                {
                    dr.Close();  // DataReader is done (read only, forward only)
                    dr.Dispose();
                }
            }

            return dict;
        }

        /// <summary>
        /// Gets a list of the column names in the order they appear in the DataTable object.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<string> GetColumnNames(DataTable dt)
        {
            List<string> columnNames = new List<string>();

            if (dt != null)
            {
                for (int i = 0; i < dt.Columns.Count; i++ )
                {
                    columnNames.Add(dt.Columns[i].ColumnName);
                }
            }

            return columnNames;
        }

        /// <summary>
        /// Gets a list of the column names in the order they appear in the DataReader object.
        /// Does not modify (advance or close) the DataReader.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static List<string> GetColumnNames(IDataReader dr)
        {
            List<string> columnNames = new List<string>();

            if (dr != null)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    columnNames.Add(dr.GetName(i));
                }
            }

            return columnNames;
        }


        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// Note that DbDataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [Obsolete("Use CollectionUtil<T>.GetColumnFromDataReader(...) instead.")]
        public static List<object> GetColumnFromDataReader(DbDataReader dr, string columnName)
        {
            List<object> column = new List<object>();

            try
            {
                while (dr.Read())
                {
                    column.Add(dr[columnName]);
                }
            }
            finally
            {
                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();  // Data Reader is done (read only, forward only)
                    dr.Dispose();
                }
            }

            return column;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// Note that DbDataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        [Obsolete("Use CollectionUtil<T>.GetColumnFromDataReader(...) instead.")]
        public static List<object> GetColumnFromDataReader(DbDataReader dr, int columnIndex)
        {
            List<object> column = new List<object>();

            try
            {
                while (dr.Read())
                {
                    column.Add(dr[columnIndex]);
                }
            }
            finally
            {
                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();  // Data Reader is done (read only, forward only)
                    dr.Dispose();
                }
            }

            return column;
        }

        /// <summary>
        /// Runs an INSERT statement.
        /// The Dictionary needs to contain the columns as keys and values as values.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="valuesDict">Dictionary with column names and values.</param>
        /// <returns>The id of the row inserted.</returns>
        [Obsolete("Warning: This query will run in ConnectionManager's default connection.\nMake sure you're working with a single connection application or use an overloaded method that takes a connection name.")]
        public static int RunInsert(string table, Dictionary<string, object> valuesDict)
        {
            return RunInsert(table, valuesDict, string.Empty);
        }

        /// <summary>
        /// Runs an INSERT statement.
        /// The Dictionary needs to contain the columns as keys and values as values.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="valuesDict"></param>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public static int RunInsert(string table, Dictionary<string, object> valuesDict, ConnectionParameters connectionParameters)
        {
            return RunInsert(table, valuesDict, connectionParameters.Name);
        }

        /// <summary>
        /// Runs an INSERT statement.
        /// The Dictionary needs to contain the columns as keys and values as values.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="valuesDict">Dictionary with column names and values.</param>
        /// <param name="connectionName">The name of the connection previously set.</param>
        /// <returns>The id of the row inserted.</returns>
        public static int RunInsert(string table, Dictionary<string, object> valuesDict, string connectionName)
        {
            // Build sql statement
            string sql = QueryBuilder.BuildInsertStatement(table, valuesDict);

            // Execute sql command
            DbCommand cmd = ConnectionFactory.GetCommand(sql, connectionName);

            // Check if values need to be converted to SQL NULL
            ConnectionParameters connectionParameters = ConnectionFactory.GetParameters(connectionName);

            if (connectionParameters.DbFamily == DbFamily.SqlServer)
            {
                foreach (string key in valuesDict.Keys)
                {
                    object value = valuesDict[key] == null ? DBNull.Value : valuesDict[key];
                    if (value.GetType() == typeof(DateTime) && ((DateTime)value).Equals(Util.DateTimeNotSet))
                    {
                        value = DBNull.Value;
                    }

                    ((SqlCommand)cmd).Parameters.AddWithValue("@" + key, value);
                }
            }

            // Note: if an exception is thrown with the error "String or binary data would be truncated."
            //       probably means that you're trying to insert a string too long for the field, ex. a 10 character string in a varchar(5)
            cmd.ExecuteNonQuery();

            // Get the last inserted id
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT @@IDENTITY";
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();

            return id;
        }

        /// <summary>
        /// Begin a Transaction.
        /// It's up to the user to manage the Commit or Rollback.
        /// </summary>
        /// <param name="connectionParameters"></param>
        public static void BeginTransaction(ConnectionParameters connectionParameters)
        {
            BeginTransaction(connectionParameters.Name);
        }

        /// <summary>
        /// Begin a Transaction.
        /// It's up to the user to manage the Commit or Rollback.
        /// </summary>
        /// <param name="connectionName"></param>
        public static void BeginTransaction(string connectionName)
        {
            RunScript("BEGIN TRANSACTION", connectionName); 
        }

        /// <summary>
        /// Commit a Transaction previously marked with Begin.
        /// </summary>
        /// <param name="connectionParameters"></param>
        public static void CommitTransaction(ConnectionParameters connectionParameters)
        {
            CommitTransaction(connectionParameters.Name);
        }

        /// <summary>
        /// Commit a Transaction previously marked with Begin.
        /// </summary>
        /// <param name="connectionName"></param>
        public static void CommitTransaction(string connectionName)
        {
            RunScript("COMMIT TRANSACTION", connectionName);
        }

        /// <summary>
        /// Rollback a Transaction previously marked with Begin.
        /// </summary>
        /// <param name="connectionParameters"></param>
        public static void RollbackTransaction(ConnectionParameters connectionParameters)
        {
            RollbackTransaction(connectionParameters.Name);
        }

        /// <summary>
        /// Rollback a Transaction previously marked with Begin.
        /// </summary>
        /// <param name="connectionName"></param>
        public static void RollbackTransaction(string connectionName)
        {
            RunScript("ROLLBACK TRANSACTION", connectionName);
        }

        /// <summary>
        /// Returns true if the object is null or DBNull.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(object obj)
        {
            return obj == null || obj.GetType() == typeof(System.DBNull);
        }

        /// <summary>
        /// Returns true if the object is null, DBNull or it's a string and is empty.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(object obj)
        {
            return IsNullOrEmpty(obj, false);
        }

        /// <summary>
        /// Returns true if the object is null, DBNull or it's a string and is empty.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="trim">Whether or not to trim the string before checking if it's empty.</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(object obj, bool trim)
        {
            return IsNull(obj) ||
                (obj.GetType() == typeof(string) && string.IsNullOrEmpty(trim ? ((string)obj).Trim() : (string)obj));
        }

        /// <summary>
        /// Returns true if options has the given option, false otherwise.
        /// </summary>
        /// <param name="options">The full set of options from a bitwise OR operation.</param>
        /// <param name="option">The desired option to check.</param>
        /// <returns></returns>
        public static bool HasDbQueryOption(DbQueryOptions options, DbQueryOptions option)
        {
            return (options & option) == option;
        }
    }
}
