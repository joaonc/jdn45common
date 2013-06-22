using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.InteropServices;

namespace Jdn45Common
{
    /// <summary>  
    /// Just like a System.Collections.Generic.KeyValuePair,
    /// but the XmlSerializer will serialize the Key and Value properties.
    /// </summary>
    // Code taken from http://ianfnelson.com/blog/a-serializeable-keyvaluepair-class/
    public class KeyValuePairSerializable<K, V>  
    {  
        private K key;
        private V value;

        public KeyValuePairSerializable()
        {
        }

        public KeyValuePairSerializable(KeyValuePair<K, V> kvPair)
        {
            this.key = kvPair.Key;
            this.value = kvPair.Value;
        }

        public KeyValuePairSerializable(K key, V value)
        {  
           this.key = key;
           this.value = value;
        }  
   
        public override string ToString()
        {  
           StringBuilder sb = new StringBuilder();
           sb.Append( '[' );
           if ( this.Key != null )
           {  
              sb.Append( this.Key.ToString() );
           }  
           sb.Append( ", " );
           if ( this.Value != null )
           {  
              sb.Append( this.Value.ToString() );
           }  
           sb.Append( ']' );
           return sb.ToString();
        }

        /// <summary>
        /// Gets the Key in the Key/Value Pair
        /// </summary>
        public K Key
        {
            get { return key; }
            set { key = value;  }
        }

        /// <summary>  
        /// Gets the Value in the Key/Value Pair
        /// </summary>  
        public V Value
        {
            get { return this.value; }
            set { this.value = value; }
        }   
    }

    public class KeyValuePairCollection<K, V> : Collection<KeyValuePairSerializable<K, V>>
    {
        public void Add(K key, V value)
        {
            this.Add(new KeyValuePairSerializable<K, V>(key, value));
        }

        public List<V> GetValuesByKey(K key)
        {
            List<V> valueList = new List<V>();
            foreach (KeyValuePairSerializable<K, V> kv in this)
            {
                if (kv.Key.Equals(key))
                {
                    valueList.Add(kv.Value);
                }
            }

            return valueList;
        }

        public List<K> GetKeysWithValue(V value)
        {
            List<K> keyList = new List<K>();
            foreach (KeyValuePairSerializable<K, V> kv in this)
            {
                if (kv.Value.Equals(value))
                {
                    keyList.Add(kv.Key);
                }
            }

            return keyList;
        }
    }

    public class ReadOnlyKeyValuePairCollection<K, V> : ReadOnlyCollection<KeyValuePairSerializable<K, V>>
    {
        public ReadOnlyKeyValuePairCollection(IList<KeyValuePairSerializable <K, V>> list) : base(list) { }
    }

    public class KeyValuePairKeyedCollection<K, V> : KeyedCollection<K, KeyValuePairSerializable<K, V>>
    {
        protected override K GetKeyForItem(KeyValuePairSerializable<K, V> item)
        {
            return item.Key;
        }

        public void Add(K key, V value)
        {
            this.Add(new KeyValuePairSerializable<K, V>(key, value));
        }
    }
}
