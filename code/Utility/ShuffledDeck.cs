using System;
using System.Collections.Generic;
using System.Linq;

namespace Garryware;

public class ShuffledDeck<T>
{
    private readonly List<T> allElements = new();
    private readonly List<T> shuffledElements = new();
    private readonly Random random = new();

    public int Count => allElements.Count;
    public int Remaining => shuffledElements.Count;
    
    public void Add(T element, int count = 1)
    {
        for (int i = 0; i < count; ++i)
        {
            allElements.Add(element);
        }
    }

    public void AddRange(IEnumerable<T> list, int count = 1)
    {
        foreach (var element in list)
        {
            Add(element, count);
        }
    }
    
    public T Next()
    {
        if (shuffledElements.Count == 0)
        {
            Shuffle(true);
        }
        
        var element = shuffledElements.First();
        shuffledElements.RemoveAt(0);
        return element;
    }

    public void Shuffle(bool returnCards = true)
    {
        if (returnCards || shuffledElements.Count < 2)
        {
            shuffledElements.Clear();
            shuffledElements.AddRange(allElements);
        }
        Shuffle(shuffledElements);
    }
    
    private void Shuffle(IList<T> list)  
    {  
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

}
