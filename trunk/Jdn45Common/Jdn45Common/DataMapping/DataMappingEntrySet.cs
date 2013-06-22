using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Jdn45Common.DataMapping
{
    /// <summary>
    /// Set of DataMappingEntry objects.
    /// The DataMappingEntry are considered unique by the NameFrom field, ie, there can be only one entry for each NameFrom value (case sensitive).
    /// </summary>
    public class DataMappingEntrySet
    {
        List<DataMappingEntry> dataMappingEntries;
        List<string> toList;

        public DataMappingEntrySet()
        {
            dataMappingEntries = new List<DataMappingEntry>();
            toList = new List<string>();
        }

        public DataMappingEntrySet(List<string> fromList)
        {
            SetDataMappingEntries(fromList, new List<string>());
        }

        public DataMappingEntrySet(List<string> fromList, List<string> toList)
        {
            SetDataMappingEntries(fromList, toList);
        }

        public DataMappingEntrySet(List<DataMappingEntry> dataMappingEntries, List<string> toList)
        {
            SetDataMappingEntries(dataMappingEntries, toList);
        }

        /// <summary>
        /// Gets or sets the data mapping entries.
        /// </summary>
        public List<DataMappingEntry> DataMappingEntries
        {
            get { return dataMappingEntries; }
            set
            {
                SetDataMappingEntries(value, null);
            }
        }

        /// <summary>
        /// Gets or sets the available mappings in the To column.
        /// </summary>
        public List<string> ToList
        {
            get { return toList; }
            set
            {
                SetDataMappingEntries((List<DataMappingEntry>)null, value);
            }
        }

        public void SetDataMappingEntries(List<DataMappingEntry> dataMappingEntries, List<string> toList)
        {
            if (dataMappingEntries == null && toList == null)
            {
                throw new Exception("Both data mapping entries and to list cannot be null.");
            }

            // Use aux in case there's an exception later we don't want to change the current values
            List<string> toListAux = this.toList;
            if (toList != null)
            {
                toListAux = toList;
            }

            // Verify
            if (dataMappingEntries != null)
            {
                foreach (DataMappingEntry entry in dataMappingEntries)
                {
                    if (entry == null)
                    {
                        throw new Exception("Data mapping entry cannot be null.");
                    }
                    if (string.IsNullOrEmpty(entry.NameFrom))
                    {
                        throw new Exception("NameTo in data mapping entry needs to be specified.");
                    }
                    if (entry.IsMapped() && !NameToExists(entry.NameTo, toListAux))
                    {
                        throw new Exception(string.Format(
                            "Data mapping {0} has a ToName column {1} that doesn't exist in the list of columns to map to.",
                            entry.NameFrom, entry.NameTo));
                    }
                }
            }
            else
            {
                RemoveMappingsTo();
            }

            // Set the new values
            if (toList != null)
            {
                this.toList = toList;
            }
            if (dataMappingEntries != null)
            {
                this.dataMappingEntries = dataMappingEntries;
            }
        }


        /// <summary>
        /// Sets the data mapping from two lists.
        /// No changes if parameters are null.
        /// </summary>
        /// <param name="columnsFrom">The From column. Can be null for no changes.</param>
        /// <param name="columnsTo">The To column. Can be null for no changes.</param>
        public void SetDataMappingEntries(List<string> columnsFrom, List<string> columnsTo)
        {
            List<DataMappingEntry> dataMappingEntriesAux = dataMappingEntries;
            if (columnsFrom != null)
            {
                dataMappingEntriesAux = new List<DataMappingEntry>();

                foreach (string column in columnsFrom)
                {
                    dataMappingEntriesAux.Add(new DataMappingEntry(column));
                }
            }

            SetDataMappingEntries(dataMappingEntriesAux, columnsTo);
        }

        private void RemoveMappingsTo()
        {
            if (this.dataMappingEntries != null)
            {
                foreach (DataMappingEntry entry in dataMappingEntries)
                {
                    entry.NameTo = string.Empty;
                }
            }
        }

        private bool NameToExists(string nameTo, List<string> toList)
        {
            return
                !string.IsNullOrEmpty(nameTo) &&
                toList.Find(delegate(string columnName)
                    {
                        return columnName.Equals(nameTo, StringComparison.CurrentCultureIgnoreCase);
                    }) != null;
        }

        public bool NameToExists(string nameTo)
        {
            return NameToExists(nameTo, toList);
        }

        /// <summary>
        /// Returns true if the item already exists.
        /// Only NameFrom is checked, not the rest of the properties.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Exists(DataMappingEntry item)
        {
            return dataMappingEntries.Exists(delegate(DataMappingEntry entry)
                {
                    return entry.NameFrom.Equals(item.NameFrom);
                });
        }

        /// <summary>
        /// Throws if the item has invalid data to be added or updated, does nothing if everything is ok.
        /// </summary>
        /// <param name="item"></param>
        protected void CheckItem(DataMappingEntry item)
        {
            if (item == null)
            {
                throw new Exception("DataMappingEntry object cannot be null.");
            }

            if (string.IsNullOrEmpty(item.NameFrom))
            {
                throw new Exception("NameFrom needs to be specified in the DataMappingEntry object when adding to the list of entries.");
            }
        }

        public void Add(DataMappingEntry item)
        {
            CheckItem(item);

            if (Exists(item))
            {
                throw new Exception(string.Format(
                    "DataMappingEntry already exists: {0}\nUse Update(...) or AddOrUpdate(...) to set a new value.",
                    item.NameFrom));
            }

            dataMappingEntries.Add(item);
        }

        public void Update(string nameFrom, string nameTo)
        {
            int index = FindIndexByNameFrom(nameFrom);

            if (index < 0)
            {
                throw new Exception(string.Format(
                    "DataMappingEntry doesn't exist to be updated: {0}\nUse Add(...) or AddOrUpdate(...) to insert a new entry.",
                    nameFrom));
            }

            dataMappingEntries[index].NameTo = nameTo;
        }

        public void Update(DataMappingEntry item)
        {
            CheckItem(item);

            int index = FindIndexByNameFrom(item.NameFrom);

            if (index < 0)
            {
                throw new Exception(string.Format(
                    "DataMappingEntry doesn't exist to be updated: {0}\nUse Add(...) or AddOrUpdate(...) to insert a new entry.",
                    item.NameFrom));
            }

            dataMappingEntries[index] = item;
        }

        public void AddOrUpdate(DataMappingEntry item)
        {
            CheckItem(item);

            int index = FindIndexByNameFrom(item.NameFrom);

            if (index < 0)
            {
                // Add
                dataMappingEntries.Add(item);
            }
            else
            {
                // Update
                dataMappingEntries[index] = item;
            }
        }

        public DataMappingEntry FindByNameFrom(string nameFrom)
        {
            return dataMappingEntries.Find(delegate(DataMappingEntry entry)
                {
                    return entry.NameFrom.Equals(nameFrom);
                });
        }

        /// <summary>
        /// Gets the current mappings for the "to" value.
        /// Note that there can be more than one mapping.
        /// </summary>
        /// <param name="nameTo"></param>
        /// <returns></returns>
        public List<DataMappingEntry> FindByNameTo(string nameTo)
        {
            return dataMappingEntries.FindAll(delegate(DataMappingEntry entry)
                {
                    return entry.NameTo.Equals(nameTo);
                });
        }

        /// <summary>
        /// Returns a list of mandatory items (true) or non mandatory items (false).
        /// </summary>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        public List<DataMappingEntry> GetMandatory(bool mandatory)
        {
            return dataMappingEntries.FindAll(delegate(DataMappingEntry entry)
                {
                    return entry.Mandatory.Equals(mandatory);
                });
        }

        /// <summary>
        /// Returns a list of mandatory items that have not been mapped yet (thus breaking business rules).
        /// </summary>
        /// <returns></returns>
        public List<DataMappingEntry> GetMandatoryNotMapped()
        {
            return dataMappingEntries.FindAll(delegate(DataMappingEntry entry)
                {
                    return entry.Mandatory && !entry.IsMapped();
                });
        }

        /// <summary>
        /// Returns a list of mapped items with the given mapping status.
        /// </summary>
        /// <param name="mapped"></param>
        /// <returns></returns>
        public List<DataMappingEntry> GetMapped(DataMappingStatus dataMappingStatus)
        {
            return dataMappingEntries.FindAll(delegate(DataMappingEntry entry)
                {
                    return entry.GetStatus() == dataMappingStatus;
                });
        }

        public int FindIndexByNameFrom(string nameFrom)
        {
            return dataMappingEntries.FindIndex(delegate(DataMappingEntry entry)
                {
                    return entry.NameFrom.Equals(nameFrom);
                });
        }

        public List<string> GetNameFromList()
        {
            List<string> nameFromList = new List<string>();

            foreach (DataMappingEntry item in dataMappingEntries)
            {
                nameFromList.Add(item.NameFrom);
            }

            return nameFromList;
        }

        /// <summary>
        /// Returns a list of headers after conversion with this mapping.
        /// The list returned maintains the order.
        /// Headers not mapped are included with the original name.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public List<string> ConvertHeaders(List<string> headers)
        {
            if (CollectionUtil<string>.HasDuplicates(headers))
            {
                throw new Exception("Duplicate headers exist.");
            }

            List<DataMappingEntry> mappingEntriesMandatoryNotMapped = GetMandatoryNotMapped();
            if (mappingEntriesMandatoryNotMapped.Count > 0)
            {
                throw new Exception(string.Format(
                    "The following mandatory mapping{0} not been set:\n{1}",
                    mappingEntriesMandatoryNotMapped.Count == 1 ? " has" : "s have",
                    StringUtil.ToString<DataMappingEntry>(mappingEntriesMandatoryNotMapped)));
            }

            List<string> converted = new List<string>(headers.Count);
            List<DataMappingEntry> mappingEntriesMandatory = new List<DataMappingEntry>();
            for (int i = 0; i < headers.Count; i++)
            {
                List<DataMappingEntry> dataMappingEntries = FindByNameTo(headers[i]);
                if (dataMappingEntries == null || dataMappingEntries.Count == 0)
                {
                    converted.Add(headers[i]);  // Not mapped
                }
                else if (dataMappingEntries.Count > 1)
                {
                    throw new Exception(string.Format(
                        "{0} mappings found for {1}. There should be only one.",
                        dataMappingEntries.Count, headers[i]));
                }
                else
                {
                    DataMappingEntry dataMapping = dataMappingEntries[0];
                    converted.Add(dataMapping.NameFrom);
                    if (dataMapping.Mandatory)
                    {
                        mappingEntriesMandatory.Add(dataMapping);  // Keep tabs on mandatory mappings to make sure all got mapped
                    }
                }
            }

            // Check if there's any mandatory mapping that didnt get mapped
            if (!CollectionUtil<DataMappingEntry>.ContainsAll(mappingEntriesMandatory, GetMandatory(true)))
            {
                throw new Exception("Not all mandatory mappings were mapped");
            }

            return converted;
        }

        /// <summary>
        /// Convert the data table using the current mappings.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="includeNonMappedColumns"></param>
        /// <returns></returns>
        public DataTable ConvertDataTable(DataTable source, bool includeNonMappedColumns)
        {
            // Check that source table does NOT contain duplicate columns
            if (CollectionUtil<DataColumn>.HasDuplicates(DataUtil.GetColumns(source), delegate(DataColumn c1, DataColumn c2)
                {
                    return c1.ColumnName.CompareTo(c2.ColumnName);
                }))
            {
                throw new Exception("Ficheiro de dados contem colunas duplicadas.");
            }

            // Check that source table contains all the columns that are set as mapped
            List<string> columnsMappedNotPresent = new List<string>();
            foreach (DataMappingEntry entry in GetMapped(DataMappingStatus.Mapped))
            {
                if (!source.Columns.Contains(entry.NameTo))
                {
                    columnsMappedNotPresent.Add(entry.NameTo);
                }
            }
            if (columnsMappedNotPresent.Count > 0)
            {
                throw new Exception(
                    "As seguintes colunas estão mapeadas a dados mas não existem na tabela:\n" + StringUtil.ToString<string>(columnsMappedNotPresent));
            }

            // The index of the list is the index in the converted table
            // In the KV Pair, the Key is the index in DataMappingEntries, the Value is the index in the source table
            List<KeyValuePair<int, int>> columnIndexConverter = new List<KeyValuePair<int, int>>();

            // Initialize columns and create index map
            // If includeNonMappedColumns is false, then the final table may not include all columns in FromList
            for (int i = 0; i < DataMappingEntries.Count; i++)
            {
                DataMappingEntry entry = DataMappingEntries[i];
                if (entry.IsMapped())
                {
                    columnIndexConverter.Add(new KeyValuePair<int,int>(i, source.Columns.IndexOf(entry.NameTo)));
                }
                else if (includeNonMappedColumns)  // Only add this column if requested
                {
                    columnIndexConverter.Add(new KeyValuePair<int, int>(i, -1));
                }
            }

            // Initialize converted data table
            DataTable dtConverted = new DataTable(source.TableName + "Converted");
            for (int i = 0; i < columnIndexConverter.Count; i++)
            {
                Type type = columnIndexConverter[i].Value == -1 ? typeof(string) : source.Columns[columnIndexConverter[i].Value].DataType;
                dtConverted.Columns.Add(new DataColumn(DataMappingEntries[columnIndexConverter[i].Key].NameFrom, type));
            }

            // Convert the data
            foreach (DataRow sourceRow in source.Rows)
            {
                DataRow convertedRow = dtConverted.NewRow();
                for (int i = 0; i < columnIndexConverter.Count; i++)
                {
                    if (columnIndexConverter[i].Value != -1)  // It's -1 when there's no mapping and includeNonMappedColumns is true
                    {
                        convertedRow[i] = sourceRow[columnIndexConverter[i].Value];
                    }
                }

                dtConverted.Rows.Add(convertedRow);
            }

            return dtConverted;
        }
    }
}
