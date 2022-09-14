using System.Collections.Generic;
using Sandbox;

namespace Garryware;

public static class ListExtensions
{
    public static void AddUnique<T>(this List<T> list, T element)
    {
        if(!list.Contains(element))
            list.Add(element);
    }
    
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Rand.Int(n);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
}
