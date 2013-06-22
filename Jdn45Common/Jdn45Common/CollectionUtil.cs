using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Jdn45Common
{

    /// <summary>
    /// Functions that help with the manipulation of collections.
    /// </summary>
    public static class CollectionUtil<T>
    {
        public static bool HasDuplicates(IList<T> list)
        {
            // Low performance algorithm written in a hurry
            // TODO: Add more performant algorithm (maybe with sorting first), even if it takes more memory
            //       Most likely call the HasDuplicates(...) implementation that takes a comparison as parameter
            //       and call it with the default comparison (from comparer)
            for (int i=0; i<list.Count; i++)
            {
                for (int j = i+1; j < list.Count; j++)
                {
                    if (list[i].Equals(list[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasDuplicates(IEnumerable<T> enumerable, Comparison<T> comparison)
        {
            List<T> list = new List<T>(enumerable);
            if (list.Count <= 1)
            {
                return false;
            }

            list.Sort(comparison);
            for (int i = 1; i < list.Count; i++)
            {
                if (comparison(list[i - 1], list[i]) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Casts lists from one type to another.
        /// Attention: May throw at runtime if cast is invalid.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> CastList(IList<object> list)
        {
            List<T> casted = new List<T>();
            foreach (object obj in list)
            {
                casted.Add((T)obj);
            }

            return casted;
        }

        /// <summary>
        /// Creates a list with the unique elements in the given list, in other words, the set.
        /// Unfortunately, C# 2.0 doesn't have sets.
        /// Note: The filtering is done using .Equals() at the object T level, not with the list's .Contains()
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> GetUniques(IList<T> list)
        {
            List<T> setList = new List<T>();
            foreach (T item in list)
            {
                if (item != null && setList.Find(delegate(T obj)
                    {
                        return item.Equals(obj);
                    }) == null)
                {
                    setList.Add(item);
                }
            }

            return setList;
        }

        /// <summary>
        /// Removes all elements in list B from list A.
        /// Modifies listA and returns the number of elements removed from it.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static int Subtract(List<T> listA, List<T> listB)
        {
            int removed = 0;
            foreach(T objB in listB)
            {
                removed += listA.RemoveAll(delegate(T objA)
                {
                    return objA.Equals(objB);
                });
            }

            return removed;
        }

        /// <summary>
        /// Returns whether list A contains any of the elements in list B.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool ContainsAny(List<T> listA, List<T> listB)
        {
            return listB.Find(delegate(T objB)
                {
                    return listA.Find(delegate(T objA)
                        {
                            return objA.Equals(objB);
                        }) != null;
                }) != null;
        }

        /// <summary>
        /// Returns whether all elements in list B exist in list A.
        /// List A may contain other elements that don't exist in B.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool ContainsAll(List<T> listA, List<T> listB)
        {
            bool containsAll = true;

            foreach (T objB in listB)
            {
                if (!listA.Contains(objB))
                {
                    containsAll = false;
                    break;
                }
            }

            return containsAll;
        }

        /// <summary>
        /// Returns whether list A and list B contain the same elements.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool HaveSameElements(List<T> listA, List<T> listB)
        {
            if (listA.Count != listB.Count)
            {
                return false;
            }

            throw new Exception("Needs better implementation. Algorithm may return false positives.");  // TODO: fix
            List<T> subtracted = new List<T>(listA);
            Subtract(subtracted, listB);

            return subtracted.Count == 0;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// Note that SqlDataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataReader(IDataReader dr, string columnName)
        {
            List<T> column = new List<T>();

            if (dr != null)
            {
                try
                {
                    while (dr.Read())
                    {
                        column.Add((T)dr[columnName]);
                    }
                }
                finally
                {
                    if (!dr.IsClosed)
                    {
                        dr.Close();  // Data Reader is done (read only, forward only)
                        dr.Dispose();
                    }
                }
            }

            return column;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// Note that SqlDataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataReader(IDataReader dr, int columnIndex)
        {
            List<T> values = new List<T>();

            if (dr != null)
            {
                try
                {
                    while (dr.Read())
                    {
                        values.Add((T)dr[columnIndex]);
                    }
                }
                finally
                {
                    if (!dr.IsClosed)
                    {
                        dr.Close();  // Data Reader is done (read only, forward only)
                        dr.Dispose();
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// Note that this will include nulls. Use GetColumnFromDataTable(DataTable dt, string columnName, bool includeNulls) to filter out nulls.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataTable(DataTable dt, string columnName)
        {
            return GetColumnFromDataTable(dt, columnName, true);
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnName"></param>
        /// <param name="includeNulls"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataTable(DataTable dt, string columnName, bool includeNulls)
        {
            List<T> values = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                if (Db.DbUtil.IsNull(row[columnName]))
                {
                    if (includeNulls)
                    {
                        values.Add(default(T));
                    }
                }
                else
                {
                    values.Add((T)row[columnName]);
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataRows(IEnumerable<DataRow> rows, string columnName)
        {
            return GetColumnFromDataRows(rows, columnName, true);
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columnName"></param>
        /// <param name="includeNulls"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataRows(IEnumerable<DataRow> rows, string columnName, bool includeNulls)
        {
            List<T> values = new List<T>();

            foreach (DataRow row in rows)
            {
                if (Db.DbUtil.IsNull(row[columnName]))
                {
                    if (includeNulls)
                    {
                        values.Add(default(T));
                    }
                }
                else
                {
                    values.Add((T)row[columnName]);
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the values of one of the columns and returns them in a List.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static List<T> GetColumnFromDataTable(DataTable dt, int columnIndex)
        {
            List<T> values = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                values.Add((T)row[columnIndex]);
            }

            return values;
        }

        /// <summary>
        /// Returns True if the Enumerable has at least N items, False otherwise.
        /// </summary>
        /// <param name="n">Number of items to check. Needs to be greater or equal than 0.</param>
        /// <param name="enumerable">Enumerable to check.</param>
        /// <returns></returns>
        public static bool EnumerableHasNOrMoreItems(int n, IEnumerable<T> enumerable)
        {
            int i = 0;
            bool hasNOrMore = false;
            foreach (T t in enumerable)
            {
                if (++i >= n)  // > for the n=0 case
                {
                    hasNOrMore = true;
                    break;
                }
            }

            return hasNOrMore;
        }

        /// <summary>
        /// Returns the number of elements in the enumerable.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static int EnumerableCount(IEnumerable<T> enumerable)
        {
            int i = 0;
            foreach (T t in enumerable)
            {
                i++;
            }

            return i;
        }

        /// <summary>
        /// Returns a new list with the items filtered.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="filterType">Whether to keep the items in the filter (remove all others) or remove them (keep all others).</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<T> Filter(List<T> list, Util.FilterType filterType, List<T> filter)
        {
            List<T> filteredList = list.FindAll(delegate(T item)
            {
                if (filterType == Util.FilterType.Keep)
                {
                    return filter.Contains(item);
                }
                if (filterType == Util.FilterType.Remove)
                {
                    return !filter.Contains(item);
                }

                throw new Exception("FilterType not known: " + filterType.ToString());
            });

            if (filteredList == null)
            {
                filteredList = new List<T>();
            }

            return filteredList;
        }
    }
}
