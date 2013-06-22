using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.DataMapping
{
    public class DataMappingEntry : IComparable
    {
        #region Static section
        /// <summary>
        /// Indicates that there's no mapping for this column (it has been set to no mapping).
        /// </summary>
        public static readonly string MappingDoesntExist = "-";

        /// <summary>
        /// Returns true if the mapping is valid.
        /// A mapping is considered valid if it's not null or blank (not mapped) and
        /// it's not MappingDoesntExist (set as not mapped).
        /// </summary>
        /// <param name="dataMappingEntry"></param>
        /// <returns></returns>
        public static bool IsValidMapping(DataMappingEntry dataMappingEntry)
        {
            return dataMappingEntry != null && !string.IsNullOrEmpty(dataMappingEntry.NameFrom) && IsValidMapping(dataMappingEntry.NameTo);
        }

        /// <summary>
        /// Returns true if the mapping is valid.
        /// A mapping is considered valid if it's not null or blank (not mapped) and
        /// it's not MappingDoesntExist (set as not mapped).
        /// </summary>
        /// <param name="mappingValue"></param>
        /// <returns></returns>
        public static bool IsValidMapping(string mappingValue)
        {
            return !string.IsNullOrEmpty(mappingValue) && !mappingValue.Equals(MappingDoesntExist);
        }

        public static bool IsSetToNotMapped(string mappingValue)
        {
            return mappingValue.Equals(MappingDoesntExist);
        }
        #endregion

        private bool mandatory;
        private string nameFrom;
        private string nameTo;
        private string info;

        public DataMappingEntry()
        {
            Mandatory = false;
        }

        public DataMappingEntry(string nameFrom)
        {
            NameFrom = nameFrom;
            Mandatory = false;
        }

        public DataMappingEntry(string nameFrom, string nameTo)
        {
            NameFrom = nameFrom;
            NameTo = nameTo;
            Mandatory = false;
        }

        public DataMappingEntry(string nameFrom, string nameTo, bool mandatory, string info)
        {
            NameFrom = nameFrom;
            NameTo = nameTo;
            Mandatory = mandatory;
            Info = info;
        }

        /// <summary>
        /// The name of the column in the fixed table.
        /// </summary>
        public string NameFrom
        {
            get { return nameFrom; }
            set { nameFrom = value; }
        }

        /// <summary>
        /// The name of the column in the table that is being imported.
        /// </summary>
        public string NameTo
        {
            get { return nameTo; }
            set { nameTo = value; }
        }

        /// <summary>
        /// Gets or sets whether this field is mandatory, meaning a mapping must exist and in the data table it cannot be null or empty.
        /// The default value is false.
        /// </summary>
        public bool Mandatory
        {
            get { return mandatory; }
            set { mandatory = value; }
        }

        /// <summary>
        /// Provides information about this mapping.
        /// Optional.
        /// </summary>
        public string Info
        {
            get { return info; }
            set { info = value; }
        }

        /// <summary>
        /// Gets the status of the mapping.
        /// </summary>
        /// <returns></returns>
        public DataMappingStatus GetStatus()
        {
            if (string.IsNullOrEmpty(NameTo))
            {
                return DataMappingStatus.NotSet;
            }
            else if (NameTo.Equals(DataMappingEntry.MappingDoesntExist))
            {
                return DataMappingStatus.NotMapped;
            }

            return DataMappingStatus.Mapped;
        }

        /// <summary>
        /// Returns true if this mapping is set to a valid mapping, ie,
        /// the mapping is set and it's valid.
        /// </summary>
        /// <returns></returns>
        public bool IsMapped()
        {
            return GetStatus() == DataMappingStatus.Mapped;
        }

        public override string ToString()
        {
            return string.Format("From:{0} | To:{1} | Mandatory:{2} | Mapped:{3}",
                NameFrom, NameTo, Mandatory, GetStatus());
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            DataMappingEntry dme = obj as DataMappingEntry;

            return CompareTo(dme);
        }

        public int CompareTo(DataMappingEntry dataMappingEntry)
        {
            int c = NameFrom.CompareTo(dataMappingEntry.NameFrom);
            if (c == 0)
            {
                c = NameTo.CompareTo(dataMappingEntry.NameTo);

                if (c == 0)
                {
                    c = Mandatory.CompareTo(dataMappingEntry.Mandatory);
                }

                // Info not compared
            }

            return c;
        }
    }
}
