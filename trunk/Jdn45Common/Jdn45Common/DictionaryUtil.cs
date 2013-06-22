using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common
{
    public static class DictionaryUtil<TKey, TValue>
    {
        /// <summary>
        /// Returns the first key whose value matches the given value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TKey FindKeyByValue(IDictionary<TKey, TValue> dict, TValue value)
        {
            foreach (TKey key in dict.Keys)
            {
                if (dict[key].Equals(value))
                {
                    return key;
                }
            }

            return default(TKey);
        }

        /// <summary>
        /// Returns all keys whose value matches the given value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<TKey> FindAllKeysByValue(IDictionary<TKey, TValue> dict, TValue value)
        {
            List<TKey> keyList = new List<TKey>();

            foreach (TKey key in dict.Keys)
            {
                if (dict[key].Equals(value))
                {
                    keyList.Add(key);
                }
            }

            return keyList;
        }

        /// <summary>
        /// Adds the given key/value pair or updates the value if the key already exists.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="kvPair"></param>
        public static void AddOrUpdate(IDictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvPair)
        {
            AddOrUpdate(dict, kvPair.Key, kvPair.Value);
        }

        /// <summary>
        /// Adds the given key/value pair or updates the value if the key already exists.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate(IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// Adds the given key/value pair or ignores it if key already exists.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="kvPair"></param>
        public static void AddOrIgnore(IDictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvPair)
        {
            AddOrIgnore(dict, kvPair.Key, kvPair.Value);
        }

        /// <summary>
        /// Adds the given key/value pair or ignores it if key already exists.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrIgnore(IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// Removes the key if it exists, doesn't do anything if it doesn't.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        public static void RemoveOrIgnore(IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// Returns the value for the given key or the default value for TValue type (usually null) if the key doesn't exist.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue Get(IDictionary<TKey, TValue> dict, TKey key)
        {
            return Get(dict, key, default(TValue));
        }

        /// <summary>
        /// Returns the value for the given key or the default value if the key doesn't exist.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue Get(IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Reverses the dictionary: keys become values and values become keys.
        /// May throw if there are duplicate values or the values are not hashable.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static Dictionary<TValue, TKey> Reverse(IDictionary<TKey, TValue> dict)
        {
            Dictionary<TValue, TKey> reverseDict = new Dictionary<TValue, TKey>(dict.Count);

            foreach (KeyValuePair<TKey, TValue> kv in dict)
            {
                reverseDict.Add(kv.Value, kv.Key);
            }

            return reverseDict;
        }
    }
}
