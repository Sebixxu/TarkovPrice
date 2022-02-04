using System.Collections.Generic;

namespace TarkovPrice
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate(this IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}