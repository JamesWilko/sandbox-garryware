using System.Collections.Generic;

namespace Garryware;

public static class DictionaryExtensions
{

    public static TValue EnsureKeyExists<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value = default)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
        return dictionary[key];
    }
    
}
