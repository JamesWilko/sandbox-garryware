using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Garryware;

public class ShuffledDeck<T>
{
    private readonly List<T> shuffledElements = new();
    private readonly List<T> usedElements = new();
    private readonly Random random = new();

    public int Count => shuffledElements.Count + usedElements.Count;
    public int Remaining => shuffledElements.Count;

    public ShuffledDeck()
    {
    }
    
    public ShuffledDeck(IEnumerable<T> list)
    {
        AddRange(list);
        Shuffle();
    }
    
    public void Add(T element, int count = 1)
    {
        for (int i = 0; i < count; ++i)
        {
            usedElements.Add(element);
        }
    }

    public void AddRange(IEnumerable<T> list, int count = 1)
    {
        foreach (var element in list)
        {
            Add(element, count);
        }
    }

    public void Clear()
    {
        shuffledElements.Clear();
        usedElements.Clear();
    }

    public T Next()
    {
        // If there's nothing left in the deck, add all the discarded cards and shuffle.
        // If there's one thing left, shuffle the discards then add them. This is to avoid
        // repeating the same thing twice.
        if (shuffledElements.Count == 0)
        {
            Shuffle(true);
        }
        else if (shuffledElements.Count == 1)
        {
            ShuffleList(usedElements);
            shuffledElements.AddRange(usedElements);
            usedElements.Clear();
        }

        var element = shuffledElements.First();
        shuffledElements.RemoveAt(0);
        usedElements.Add(element);
        return element;
    }

    public void Shuffle(bool returnCards = true)
    {
        if (returnCards)
        {
            shuffledElements.AddRange(usedElements);
            usedElements.Clear();
        }
        ShuffleList(shuffledElements);
    }

    private void ShuffleList(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public T GetRandomUsedElement()
    {
        return Rand.FromList(usedElements);
    }

}
