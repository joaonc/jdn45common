using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Helper class to build SQL queries.
    /// </summary>
    public static class QueryBuilder
    {
        /// <summary>
        /// Makes sure the name has square brackets "[ ]" for safe SQL string.
        /// Should be used for table and column names.
        /// </summary>
        /// <param tableName="str"></param>
        /// <returns></returns>
        public static string GetSqlSafeName(string name)
        {
            return string.Format("[{0}]", name.Trim(' ', '[', ']'));
        }

        /// <summary>
        /// Helper function to build the WHERE clause.
        /// If there's only one paramenter, returns "='value'"
        /// If there's more than one parameter, returns "in ('value1', 'value2', ..., 'valueN')"
        /// </summary>
        /// <param name="whereParams"></param>
        /// <returns></returns>
        [Obsolete("Use QueryBuilder.BuildWhere<T>(...) - Note the fieldname and options.")]
        public static string BuildWhere(params object[] whereParams)
        {
            StringBuilder sb = new StringBuilder();

            // Unboxing needed when an actual array is passed as parameter
            if (whereParams.Length > 0 && whereParams[0].GetType().IsArray)
            {
                // This casting (an others) didn't work: ((object[])(whereParams[0]))[0]
                // So going with overloading solution.
                throw new Exception("Error: use overloaded function that takes an array and not params.");
            }

            if (whereParams.Length == 0)  // in case an actual array was passed as parameter
            {
                sb.Append(" = ''");
            }
            else if (whereParams.Length == 1)
            {
                sb.Append(" = '").Append(whereParams[0].ToString()).Append("'");
            }
            else
            {
                sb.Append(" in (");
                foreach (object whereParam in whereParams)
                {
                    sb.Append("'").Append(whereParam.ToString()).Append("'");
                }
                sb.Append(')');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper function to build the WHERE clause.
        /// If there's only one paramenter, returns "='value'"
        /// If there's more than one parameter, returns "in ('value1', 'value2', ..., 'valueN')"
        /// </summary>
        /// <param name="whereParams"></param>
        /// <returns></returns>
        [Obsolete("Use QueryBuilder.BuildWhere<T>(...) - Note the fieldname and options.")]
        public static string BuildWhere(int[] whereParams)
        {
            StringBuilder sb = new StringBuilder();

            if (whereParams.Length == 0)
            {
                sb.Append(" = ''");
            }
            else if (whereParams.Length == 1)
            {
                sb.Append(" = '").Append(whereParams[0].ToString()).Append("'");
            }
            else
            {
                sb.Append(" in (");
                foreach (object whereParam in whereParams)
                {
                    sb.Append("'").Append(whereParam.ToString()).Append("'");
                }
                sb.Append(')');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper function to build the WHERE clause.
        /// You should use BuildWhere T (...) if you have access the type of whereParam at design time as that will be faster.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="whereParam"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string BuildWhere(string fieldName, object whereParam, DbQueryOptions options)
        {
            List<System.Reflection.MethodInfo> methods = new List<System.Reflection.MethodInfo>(typeof(DbUtil).GetMethods());
            List<System.Reflection.MethodInfo> buildWhereMethods = methods.FindAll(delegate(System.Reflection.MethodInfo methodInfo)
                {
                    return methodInfo.Name.Equals("BuildWhere") &&
                        methodInfo.IsGenericMethod &&
                        methodInfo.GetParameters().Length == 3;
                    //                        methodInfo.GetParameters()[1].ParameterType.IsGenericParameter &&
                    //                        methodInfo.GetParameters()[1].ParameterType.Equals(typeof(IEnumerable<>));
                });

            //System.Reflection.MethodInfo genericMethodInfo = buildWhereMethods[0].MakeGenericMethod(typeof(whereParam));

            //return genericMethodInfo.Invoke(
            return null;  // TODO: implement
        }

        /// <summary>
        /// Helper function to build the WHERE clause.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="whereParam"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string BuildWhere<T>(string fieldName, T whereParam, DbQueryOptions options)
        {
            List<T> whereParams = new List<T>(1);

            // If whereParam is null, it won't be added to the where clause
            // and the query will return none or all results (depending on SqlQueryOptions)
            if (whereParam != null)
            {
                whereParams.Add(whereParam);
            }

            return BuildWhere<T>(fieldName, whereParams, options);
        }

        /// <summary>
        /// Helper function to build the WHERE clause.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="whereParams"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string BuildWhere<T>(string fieldName, IEnumerable<T> whereParams, DbQueryOptions options)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new Exception("Field name needs to be set.");
            }

            bool likeLeft = DbUtil.HasDbQueryOption(options, DbQueryOptions.LikeLeft);
            bool likeRight = DbUtil.HasDbQueryOption(options, DbQueryOptions.LikeRight);
            bool diacriticsInsensitive = DbUtil.HasDbQueryOption(options, DbQueryOptions.DiacriticsInsensitive);
            bool diacriticsSensitive = DbUtil.HasDbQueryOption(options, DbQueryOptions.DiacriticsSensitive);
            bool caseInsensitive = DbUtil.HasDbQueryOption(options, DbQueryOptions.CaseInsensitive);
            bool caseSensitive = DbUtil.HasDbQueryOption(options, DbQueryOptions.CaseSensitive);
            bool skipApostrophe = DbUtil.HasDbQueryOption(options, DbQueryOptions.SkipApostrophe);

            string apostrophe = skipApostrophe ? "" : "'";

            // Verification
            if (diacriticsSensitive && diacriticsInsensitive)
            {
                throw new Exception(
                    "Query options cannot have diacritics sensitive and insensitive at the same time.\n" +
                    "Either one or the other or none, but not both.");
            }
            if (caseSensitive && caseInsensitive)
            {
                throw new Exception(
                    "Query options cannot have case sensitive and insensitive at the same time.\n" +
                    "Either one or the other or none, but not both.");
            }
            if ((likeLeft || likeRight) && skipApostrophe)
            {
                throw new Exception(
                    "Query options cannot have a Like and SkipApostrophe otions at the same time");
            }

            StringBuilder sb = new StringBuilder();

            // Build collation
            string collation = string.Empty;
            if (diacriticsInsensitive || diacriticsSensitive || caseInsensitive || caseSensitive)
            {
                collation = string.Format("COLLATE Latin1_General_{0}_{1} ",
                    caseSensitive ? "CS" : "CI",
                    diacriticsInsensitive ? "AI" : "AS");
            }

            // Build where part of the query
            if (CollectionUtil<T>.EnumerableHasNOrMoreItems(2, whereParams))
            {
                // Multiple items
                if (likeLeft || likeRight)
                {
                    foreach (T whereParam in whereParams)
                    {
                        sb.Append(fieldName).Append(" ");
                        sb.Append(collation);
                        sb.Append("LIKE '");
                        if (likeLeft)
                        {
                            sb.Append('%');
                        }
                        sb.Append(whereParam.ToString());
                        if (likeRight)
                        {
                            sb.Append('%');
                        }
                        sb.Append("' OR ");
                    }

                    sb.Remove(sb.Length - 4, 4);  // Remove last ' OR '
                }
                else
                {
                    // 'LIKE' not used. Use 'IN' instead of '= OR =...'
                    sb.Append(fieldName).Append(" IN (");
                    foreach (T whereParam in whereParams)
                    {
                        sb.Append(apostrophe).Append(whereParam.ToString()).Append(apostrophe).Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);  // Remove last ','
                    sb.Append(')');
                }
            }
            else if (CollectionUtil<T>.EnumerableHasNOrMoreItems(1, whereParams))
            {
                // One item
                IEnumerator<T> enumerator = whereParams.GetEnumerator();
                enumerator.MoveNext();
                T whereParam = enumerator.Current;

                if (likeLeft || likeRight)
                {
                    sb.Append(fieldName).Append(" ").Append(collation).Append("LIKE '");
                    if (likeLeft)
                    {
                        sb.Append('%');
                    }
                    sb.Append(whereParam.ToString());
                    if (likeRight)
                    {
                        sb.Append('%');
                    }
                    sb.Append('\'');
                }
                else
                {
                    sb.Append(fieldName).Append(" = ").Append(apostrophe).Append(whereParam.ToString()).Append(apostrophe);
                }
            }
            else
            {
                // No items
                if (likeLeft || likeRight)
                {
                    sb.Append(fieldName).Append(" ").Append(collation).Append("LIKE '%'");
                }
                else
                {
                    sb.Append(fieldName).Append(" ").Append(collation).Append("= ''");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the sql safe column name from the table string that may have other info.
        /// 
        /// Ex: All of the inputs below have the output "[ColumnName]"
        ///   ColumnName
        ///   [ColumnName]
        ///   t.ColumnName
        ///   t.[ColumnName]
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string ExtractColumnName(string column)
        {
            if (column.IndexOf('[') != -1)
            {
                return column.Substring(column.IndexOf('[') + 1, column.Length - column.LastIndexOf(']'));
            }

            if (column.IndexOf('.') != -1)
            {
                return column.Split('.')[1];
            }

            return GetSqlSafeName(column);
        }

        /// <summary>
        /// Creates a statement for strings in a IN part of the SQL query.
        /// Ex:
        ///     values = {'a', 'b', 'c'}
        /// Returns:
        ///     "('a', 'b', 'c')"
        ///     to be used in WHERE values IN ('a', 'b', 'c')
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string BuildInStatement(IEnumerable<string> values)
        {
            StringBuilder sb = new StringBuilder("(");

            foreach (string value in values)
            {
                sb.Append("'").Append(value).Append("', ");
            }
            sb.Remove(sb.Length - 2, 2);  // Remove the last comma and spaces
            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Replaces the character ' with ´.
        /// Useful when using straight SQL strings.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FilterValue(string str)
        {
            return str.Replace('\'', '´');
        }

        /// <summary>
        /// Creates a SELECT statement for the given dictionary.
        /// The keys are the table columns, the values are the names assigned to them.
        /// If the value is null, no alias name is given.
        /// 
        /// Ex:
        ///     columnDict = {{'empName', 'Employee Name'}, {'a.Address', 'Employee Address'}}
        /// Returns:
        ///     "select empName as 'Employee Name', a.Address as 'Employee Address'"
        /// </summary>
        /// <param name="columnDict"></param>
        /// <returns></returns>
        public static string BuildSelectStatement(Dictionary<string, string> columnDict)
        {
            StringBuilder sb = new StringBuilder("SELECT ");

            foreach (string key in columnDict.Keys)
            {
                if (string.IsNullOrEmpty(columnDict[key]))
                {
                    sb.Append(key).Append(", ");
                }
                else
                {
                    sb.Append(string.Format("{0} AS '{1}', ", key, columnDict[key]));
                }
            }
            sb.Remove(sb.Length - 2, 2);  // Remove the last comma and spaces

            return sb.ToString();
        }

        /// <summary>
        /// Creates a SELECT statement for the given enumerable.
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static string BuildSelectStatement(IEnumerable<string> columns)
        {
            Dictionary<string, string> interfaceDict = new Dictionary<string, string>();

            foreach (string column in columns)
            {
                interfaceDict.Add(column, null);
            }

            return BuildSelectStatement(interfaceDict);
        }

        /// <summary>
        /// Creates a SELECT statement for the given key/value pairs.
        /// </summary>
        /// <param name="valuesKV"></param>
        /// <returns></returns>
        public static string BuildSelectStatement(params KeyValuePair<string, string>[] valuesKV)
        {
            Dictionary<string, string> interfaceDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kv in valuesKV)
            {
                interfaceDict.Add(kv.Key, kv.Value);
            }

            return BuildSelectStatement(interfaceDict);
        }

        /// <summary>
        /// Creates an UPDATE statement for the given table.
        /// The key/value pairs need to contain the columns as keys and values as values.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="valuesKV"></param>
        /// <returns></returns>
        public static string BuildUpdateStatement(string table, params KeyValuePair<string, object>[] valuesKV)
        {
            Dictionary<string, object> interfaceDict = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kv in valuesKV)
            {
                interfaceDict.Add(kv.Key, kv.Value);
            }

            return BuildUpdateStatement(table, interfaceDict);
        }

        /// <summary>
        /// Creates an UPDATE statement for the given table.
        /// The Dictionary needs to contain the columns as keys and values as values.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="valuesDict"></param>
        /// <returns></returns>
        public static string BuildUpdateStatement(string table, Dictionary<string, object> valuesDict)
        {
            if (valuesDict.Count == 0)
            {
                throw new Exception("No key/value pairs to build update statement.");
            }

            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(GetSqlSafeName(table));
            sql.AppendLine(" SET ");

            string separator = "," + Environment.NewLine;
            foreach (string key in valuesDict.Keys)
            {
                string column = GetSqlSafeName(ExtractColumnName(key));
                object valueObj = valuesDict[key];
                if (Util.IsString(valueObj))
                {
                    valueObj = FilterValue(valueObj.ToString());
                }
                string value = GetSqlStringRepresentation(valueObj, false);

                sql.Append(string.Format("{0} = {1}{2}", column, value, separator));
            }
            sql.Remove(sql.Length - separator.Length, separator.Length);  // Remove the last comma and spaces
            sql.Append(" ");

            return sql.ToString();
        }

        /// <summary>
        /// Creates an UPDATE statement for the given table.
        /// </summary>
        /// <param name="table">The table name</param>
        /// <param name="valuesDict">Keys and values. Keys are the values of the interfaceDicts defined in the DAO objects.</param>
        /// <param name="dereferenceDict">The interfaceDict to use to derreference the key name.</param>
        /// <param name="filterTableAlias">The table alias. Can be null if there's none.</param>
        /// <returns></returns>
        public static string BuildUpdateStatement(
            string table, Dictionary<string, object> valuesDict,
            Dictionary<string, string> dereferenceDict, string filterTableAlias)
        {
            Dictionary<string, object> finalValuesDict = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(filterTableAlias))
            {
                filterTableAlias = filterTableAlias.Trim('.') + ".";
            }

            Dictionary<string, string> reverseDereferenceDict = new Dictionary<string, string>();
            foreach (string key in dereferenceDict.Keys)
            {
                reverseDereferenceDict.Add(dereferenceDict[key], key);
            }

            foreach (string key in valuesDict.Keys)
            {
                if (string.IsNullOrEmpty(filterTableAlias) ||
                    (reverseDereferenceDict.ContainsKey(key) && reverseDereferenceDict[key].StartsWith(filterTableAlias)))
                {
                    finalValuesDict.Add(reverseDereferenceDict[key], valuesDict[key]);
                }
            }

            return BuildUpdateStatement(table, finalValuesDict);
        }

        /// <summary>
        /// Returns the SQL script for an INSERT statement.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="valuesDict"></param>
        /// <returns></returns>
        public static string BuildInsertStatement(string table, Dictionary<string, object> valuesDict)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(GetSqlSafeName(table));
            sql.Append(" (");

            foreach (string key in valuesDict.Keys)
            {
                sql.Append("[").Append(key).Append("], ");
            }
            sql.Remove(sql.Length - 2, 2);  // Remove the last comma and spaces
            sql.Append(") VALUES (");
            foreach (string key in valuesDict.Keys)
            {
                sql.Append(GetSqlStringRepresentation(valuesDict[key], false));
                sql.Append(", ");
            }
            sql.Remove(sql.Length - 2, 2);  // Remove the last comma and spaces
            sql.Append(")");

            return sql.ToString();
        }

        /// <summary>
        /// Returns the SQL script to run a stored procedure.
        /// </summary>
        /// <param name="sprocName"></param>
        /// <param name="valuesDict"></param>
        /// <returns></returns>
        public static string BuildExecStoredProcedure(string sprocName, Dictionary<string, object> valuesDict)
        {
            StringBuilder sql = new StringBuilder("EXEC ");
            sql.Append(GetSqlSafeName(sprocName));
            sql.Append(" ");

            foreach (KeyValuePair<string, object> kvPair in valuesDict)
            {
                if (!kvPair.Key.StartsWith("@"))
                {
                    sql.Append("@");
                }
                sql.Append(kvPair.Key);
                sql.Append(GetSqlStringRepresentation(kvPair.Value, false));
                sql.Append(", ");
            }
            sql.Remove(sql.Length - 2, 2);  // Remove the last comma and spaces

            return sql.ToString();
        }

        /// <summary>
        /// Returns the representation of the object in a string so it can be inserted in a SQL statement.
        /// Includes single quote (') for strings.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="forceQuotes">Force single quote (') at beginning and end. If false, quotes are only returned for strings and dates.</param>
        /// <returns></returns>
        public static string GetSqlStringRepresentation(object obj, bool forceQuotes)
        {
            string sql;
            if (DbUtil.IsNull(obj))
            {
                sql = "null";
            }
            else
            {
                Type type = obj.GetType();

                if (type.IsEnum)
                {
                    throw new Exception("Enums need to be converted per EnumPersistence rules.");

                }
                else if (Util.IsBoolean(type))
                {
                    bool b = Convert.ToBoolean(obj);
                    sql = b ? "1" : "0";
                }
                else if (Util.IsIntegral(type))
                {
                    sql = obj.ToString();
                }
                else if (Util.IsNumeric(type))
                {
                    decimal d = Convert.ToDecimal(obj);
                    sql = d.ToString("#.################").Replace(',', '.');  // Assuming DB's collation has '.' as decimal separator.
                }
                else if (Util.IsDate(type))
                {
                    DateTime dt = (DateTime)obj;  // Currently only DateTime makes IsDate return true. Use Convert.ToDateTime if that's not true.
                    if (dt.Equals(Util.DateTimeNotSet))
                    {
                        sql = "null";
                    }
                    else
                    {
                        sql = string.Format("\'{0}\'", GetSqlDate(dt));
                    }
                }
                else if (Util.IsString(type))
                {
                    sql = string.Format("\'{0}\'", obj);
                }
                else
                {
                    throw new Exception("Unknown type to process to sql: " + type.Name);
                }
            }

            if (forceQuotes && !(sql.StartsWith("'") && sql.EndsWith("'")))
            {
                sql = string.Format("\'{0}\'", sql);
            }

            return sql;
        }

        /// <summary>
        /// Converts a DateTime object into a string that can be used in a SQL query.
        /// Does not include the ' at the beginning and end.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetSqlDate(DateTime dateTime)
        {
            return GetSqlDate(dateTime, DbDateOptions.Full);
        }

        /// <summary>
        /// Converts a DateTime object into a string that can be used in a SQL query.
        /// Does not include the ' at the beginning and end.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sqlDateOptions"></param>
        /// <returns></returns>
        public static string GetSqlDate(DateTime dateTime, DbDateOptions sqlDateOptions)
        {
            string dateFormat;
            string dateAppend = String.Empty;

            switch (sqlDateOptions)
            {
                case DbDateOptions.Short:
                    dateFormat = "yyyy-MM-dd";
                    break;
                case DbDateOptions.Full:
                    dateFormat = "yyyy-MM-dd HH:mm:ss";
                    break;
                case DbDateOptions.FullDayRoundDown:
                    dateFormat = "yyyy-MM-dd";
                    dateAppend = " 00:00:00";
                    break;
                case DbDateOptions.FullDayRoundUp:
                    dateFormat = "yyyy-MM-dd";
                    dateAppend = " 23:59:59";
                    break;
                default:
                    throw new Exception("Unexpected SqlDateOption " + sqlDateOptions.ToString());
            }

            dateFormat = "{0:" + dateFormat + "}" + dateAppend;
            return string.Format(dateFormat, dateTime);
        }
    }
}
