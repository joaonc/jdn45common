using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.ComponentModel;
using System.IO;
using Jdn45Common.DataMapping;

namespace Jdn45Common
{
    /// <summary>
    /// Data manipulation functions.
    /// </summary>
    public static class DataUtil
    {
        /// <summary>
        /// Sets the data in DataGridView from the DbDataReader object.
        /// 
        /// Notes:
        ///   1. There's no data binding going on
        ///   2. The data reader object is disposed
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="dataReader"></param>
        public static void SetDataGridViewData(DataGridView dataGridView, IDataReader dataReader)
        {
            SetDataGridViewData(dataGridView, dataReader, null, Util.FilterType.Keep);
        }

        /// <summary>
        /// Sets the data in DataGridView from the DataTable object.
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="dataTable"></param>
        public static void SetDataGridViewData(DataGridView dataGridView, DataTable dataTable)
        {
            SetDataGridViewData(dataGridView, dataTable, null, Util.FilterType.Keep);
        }

        /// <summary>
        /// Sets the data in DataGridView from the DbDataReader object.
        /// The columns can be filtered or null to get all columns from the data reader in the the data grid view.
        /// 
        /// Notes:
        ///   1. There's no data binding going on
        ///   2. The data reader object is disposed
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="dataReader"></param>
        /// <param name="columnFilter">The columns to filter.</param>
        /// <param name="filterType">Whether to keep the items in the filter (remove all others) or remove them (keep all others).</param>
        public static void SetDataGridViewData(DataGridView dataGridView, IDataReader dataReader, IEnumerable<string> columnFilter, Util.FilterType filterType)
        {
            // Convert data reader into data table
            DataTable dataTable = new DataTable();
            dataTable.Load(dataReader);
            dataReader.Close();
            dataReader.Dispose();

            SetDataGridViewData(dataGridView, dataTable, columnFilter, filterType);
        }

        /// <summary>
        /// Sets the data in DataGridView from the DataTable object.
        /// The columns can be filtered or null to get all columns from the data table in the the data grid view.
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="dataTable"></param>
        /// <param name="columnFilter">The columns to filter.</param>
        /// <param name="filterType">Whether to keep the items in the filter (remove all others) or remove them (keep all others).</param>
        public static void SetDataGridViewData(DataGridView dataGridView, DataTable dataTable, IEnumerable<string> columnFilter, Util.FilterType filterType)
        {
            // Clear DGV if it's not data bound. If it is, it will be re-bound later, therefore cleaning the old data.
            if (dataGridView.DataSource == null)
            {
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();
            }
            else
            {
                // Force data binding flush
                dataGridView.DataSource = null;
            }

            // Bind the data
            dataGridView.DataSource = dataTable;

            // Hide the columns filtered out
            if (columnFilter != null)
            {
                List<string> columnFilterList = new List<string>(columnFilter);
                List<string> columns = Db.DbUtil.GetColumnNames(dataTable);
                List<string> columnsToRemove = null;

                // Get a list of columns to remove
                if (filterType == Util.FilterType.Keep)
                {
                    columnsToRemove = CollectionUtil<string>.Filter(columns, Util.FilterType.Remove, columnFilterList);
                }
                else if (filterType == Util.FilterType.Remove)
                {
                    columnsToRemove = columnFilterList;
                }
                else
                {
                    throw new Exception("Unkonwn filter type: " + filterType.ToString());
                }

                // Hide the columns
                foreach (string col in columnsToRemove)
                {
                    dataGridView.Columns[col].Visible = false;
                }

                // Force column order as defined in column filter
                // but only if it's FilterType.Keep
                if (filterType == Util.FilterType.Keep)
                {
                    int i = 0;
                    foreach (string column in columnFilter)
                    {
                        dataGridView.Columns[column].DisplayIndex = i++;
                    }
                }
            }

            dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        public static void SetDataGridViewRowNumbers(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    row.HeaderCell.Value = string.Format("{0}", row.Index + 1);
                }
            }
            dataGridView.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        public static void SetPropertyGridData(PropertyGrid propertyGrid, IDictionary dictionary)
        {
            SetPropertyGridData(propertyGrid, dictionary, false);
        }

        public static void SetPropertyGridData(PropertyGrid propertyGrid, IDictionary dictionary, bool readOnly)
        {
            propertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(dictionary, readOnly);
        }

        public static DataTable JoinData(DbDataReader dr1, DbDataReader dr2, string relation)
        {
            return JoinData(dr1, dr2, new KeyValuePair<string, string>(relation, relation));
        }

        public static DataTable JoinData(DbDataReader dr1, DbDataReader dr2, KeyValuePair<string, string> relation)
        {
            // Create the DataTables
            DataTable dt1 = new DataTable("Table1");
            DataTable dt2 = new DataTable("Table2");
            dt1.Load(dr1);
            dt2.Load(dr2);

            dr1.Close();
            dr1.Dispose();
            dr2.Close();
            dr2.Dispose();

            return JoinData(dt1, dt2, relation);
        }

        public static DataTable JoinData(DataTable dt1, DataTable dt2, string relation)
        {
            return JoinData(dt1, dt2, new KeyValuePair<string, string>(relation, relation));
        }

        public static DataTable JoinData(DataTable dt1, DataTable dt2, KeyValuePair<string, string> relation)
        {
            // Check everything is ok for the relation
            if (!dt1.Columns.Contains(relation.Key) ||
                !dt2.Columns.Contains(relation.Value))
            {
                throw new Exception(string.Format(
                    "Both tables need to contain the columns in the relation (FK).\n" +
                    "Column {0} in table {1}, column {2} in table {3}",
                    relation.Key, dt1.TableName, relation.Value, dt2.TableName));
            }

            if (dt1.Columns[relation.Key].DataType != dt2.Columns[relation.Value].DataType)
            {
                throw new Exception("Data types need to match in the columns that have relation (FK).");
            }

            // Create the DataSet
            DataSet ds = new DataSet();
            ds.Tables.Add(dt1);
            ds.Tables.Add(dt2);

            // Create the DataRelation
            DataColumn table1Col = ds.Tables[0].Columns[relation.Key];
            DataColumn table2Col = ds.Tables[1].Columns[relation.Value];
            string dataRelationName = relation.Key + "_" + relation.Value;
            DataRelation dataRelation = new DataRelation(dataRelationName, table1Col, table2Col, false);
            ds.Relations.Add(dataRelation);

            // Join the two tables
            DataTable dtJoined = new DataTable("JoinedTable");

            // First, create all the columns
            // Keeping tabs where the columns are coming from for the join later in a dictionary
            // that contains the table in the key and the column names in the values
            Dictionary<DataTable, List<string>> columnSource = new Dictionary<DataTable,List<string>>();
            columnSource.Add(dt1, new List<string>());
            columnSource.Add(dt2, new List<string>());
            foreach (DataColumn column in dt1.Columns)
            {
                dtJoined.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
                columnSource[dt1].Add(column.ColumnName);
            }
            foreach (DataColumn column in dt2.Columns)
            {
                if (!dtJoined.Columns.Contains(column.ColumnName))
                {
                    dtJoined.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
                    columnSource[dt2].Add(column.ColumnName);
                }
            }
            // Then add all the rows
            foreach (DataRow row_dt1 in dt1.Rows)
            {
                DataRow[] rows_dt2 = row_dt1.GetChildRows(dataRelation);

                // It's the equivalent of an inner join
                // If there's no relating row in dt2, no row is added
                foreach (DataRow row_dt2 in rows_dt2)
                {
                    DataRow rowJoined = dtJoined.NewRow();

                    // Add the columns from both rows
                    foreach (string columnName in columnSource[dt1])
                    {
                        rowJoined[columnName] = row_dt1[columnName];
                    }
                    foreach (string columnName in columnSource[dt2])
                    {
                        rowJoined[columnName] = row_dt2[columnName];
                    }

                    dtJoined.Rows.Add(rowJoined);
                }
            }

            return dtJoined;
        }

        /// <summary>
        /// Remove columns from a Data Table.
        /// </summary>
        /// <param name="dataTable">Data Table object to be modified.</param>
        /// <param name="columnFilter">List of columns names. If null then it's a no op.</param>
        /// <param name="filterType">Whether to keep or remove the columns in the list.</param>
        public static void RemoveColumns(DataTable dataTable, IList<string> columnFilter, Util.FilterType filterType)
        {
            if (columnFilter != null)
            {
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    bool columnInList = columnFilter.Contains(dataTable.Columns[i].ColumnName);
                    if ((columnInList && filterType == Util.FilterType.Remove) ||
                        (!columnInList && filterType == Util.FilterType.Keep))
                    {
                        dataTable.Columns.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the separator in a file.
        /// This is not a very secure way to get the separator. Use only if you know the file's structure.
        /// Throws if separator was not found.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static char GetSeparator(string fileName)
        {
            // Read the first row to get column names
            StreamReader stream = new StreamReader(fileName);
            string header = stream.ReadLine();
            stream.Close();
            stream.Dispose();

            // Get the separator
            int separatorIndex = header.IndexOfAny(new char[] { ',', ';', '\t' });
            if (separatorIndex < 0)
            {
                throw new Exception("Separator not found automatically in file " + fileName);
            }

            return header[separatorIndex];
        }

        public static string[] GetHeaders(string fileName, char separator)
        {
            StreamReader stream = new StreamReader(fileName, Encoding.Default);
            string firstLine = stream.ReadLine();
            stream.Close();
            stream.Dispose();

            return firstLine.Split(separator);
        }

        /// <summary>
        /// Loads the contents of a file into a DataTable. All columns will be strings.
        /// The first line needs to contain the header and the column separator is obtained automatically.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DataTable Load(string fileName)
        {
            return Load(fileName, GetSeparator(fileName));
        }

        /// <summary>
        /// Loads the contents of a file into a DataTable. All columns will be strings.
        /// The first line needs to contain the header and the column separator is obtained automatically.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dataMapping">Column mapping.</param>
        /// <returns></returns>
        public static DataTable Load(string fileName, DataMappingEntrySet dataMapping)
        {
            return Load(fileName, GetSeparator(fileName), dataMapping);
        }

        /// <summary>
        /// Loads the contents of a file into a DataTable. All columns will be strings.
        /// The first line needs to contain the header.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="separator">Character that represents the column separation.</param>
        /// <returns></returns>
        public static DataTable Load(string fileName, char separator)
        {
            return Load(fileName, separator, null);
        }

        /// <summary>
        /// Loads the contents of a file into a DataTable. All columns will be strings.
        /// The first line needs to contain the header.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="separator">Character that represents the column separation.</param>
        /// <param name="dataMapping">Column mapping.</param>
        /// <returns></returns>
        public static DataTable Load(string fileName, char separator, DataMappingEntrySet dataMapping)
        {
            StreamReader stream = new StreamReader(fileName, Encoding.Default);
            DataTable dt = null;

            try
            {
                // Initialize header (column names)
                List<string> columnNames;
                try
                {
                    string firstLine = stream.ReadLine();
                    columnNames = new List<string>(firstLine.Split(separator));
                }
                catch (Exception ex)
                {
                    throw new Exception("Error processing the header", ex);
                }

                if (CollectionUtil<string>.HasDuplicates(columnNames))
                {
                    throw new Exception(string.Format("Duplicate columns in data file {0}", fileName));
                }

                if (dataMapping != null)
                {
                    columnNames = dataMapping.ConvertHeaders(columnNames);
                }

                dt = new DataTable();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    dt.Columns.Add(columnNames[i], typeof(string));
                }

                // Read the remaining file
                int lineNr = 1;
                while (stream.Peek() > -1)
                {
                    string line = stream.ReadLine();
                    string[] columns = line.Split(separator);

                    if (columns.Length != columnNames.Count)
                    {
                        throw new Exception(string.Format(
                            "Error in line {0}: {1} columns found, expected {2}",
                            lineNr, columns.Length, columnNames.Count));
                    }

                    DataRow row = dt.NewRow();
                    try
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            row[i] = columns[i];
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error loading table in line {0}.", lineNr), ex);
                    }

                    dt.Rows.Add(row);
                    lineNr++;
                }
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }

            return dt;
        }

        /// <summary>
        /// Gets a dictionary from the row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetDictionary(DataRow row)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (DataColumn column in row.Table.Columns)
            {
                dict.Add(column.ColumnName, row[column.ColumnName]);
            }

            return dict;
        }

        /// <summary>
        /// Gets the columns from the data table in a list.
        /// Sometimes necessary as Columns does not implement IEnumerable.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<DataColumn> GetColumns(DataTable dt)
        {
            List<DataColumn> columns = new List<DataColumn>();
            foreach (DataColumn column in dt.Columns)
            {
                columns.Add(column);
            }

            return columns;
        }
    }

    /// <summary>
    /// Custom Type Descriptor to be able to place IDictionary in a PropertyForm.
    /// </summary>
    // Code taken from http://www.differentpla.net/content/2005/02/using-propertygrid-with-dictionary
    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        IDictionary _dictionary;
        bool _readOnly;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
            _readOnly = false;
        }

        public DictionaryPropertyGridAdapter(IDictionary d, bool readOnly)
        {
            _dictionary = d;
            _readOnly = readOnly;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key, _readOnly));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }
    }

    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        IDictionary _dictionary;
        object _key;
        bool _readOnly;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
            _readOnly = false;
        }

        internal DictionaryPropertyDescriptor(IDictionary d, object key, bool readOnly)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
            _readOnly = readOnly;
        }

        public override Type PropertyType
        {
            get { return _dictionary[_key].GetType(); }
        }

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly
        {
            get { return _readOnly; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
