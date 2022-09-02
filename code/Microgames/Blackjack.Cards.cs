namespace Garryware.Microgames;

public static class BlackjackCards
{
    public struct CardSet
    {
        public int[] CardValues;
        
        public CardSet(params int[] cardValues)
        {
            CardValues = cardValues;
        }
    }

    public static CardSet[] ThreeCardSets =
    {
        new(6, 7, 8),
        new(5, 7, 9),
        new(5, 6, 10),
        new(4, 8, 9),
        new(4, 7, 10),
        new(4, 6, 11),
        new(3, 8, 10),
        new(3, 7, 11),
        new(2, 9, 10),
        new(2, 8, 11),
        new(1, 9, 11),
    };
    
    public static CardSet[] FourCardSets =
    {
        new(3, 5, 6, 7),
        new(3, 4, 6, 8),
        new(3, 4, 5, 9),
        new(2, 5, 6, 8),
        new(2, 4, 7, 8),
        new(2, 4, 6, 9),
        new(2, 4, 5, 10),
        new(2, 3, 7, 9),
        new(2, 3, 6, 10),
        new(2, 3, 5, 11),
        new(1, 5, 7, 8),
        new(1, 5, 6, 9),
        new(1, 4, 7, 9),
        new(1, 4, 6, 10),
        new(1, 4, 5, 11),
        new(1, 3, 8, 9),
        new(1, 3, 7, 10),
        new(1, 3, 6, 11),
        new(1, 2, 8, 10),
        new(1, 2, 7, 11),
    };
    
    public static CardSet[] FiveCardSets =
    {
        new(2, 3, 4, 5, 7),
        new(1, 3, 4, 6, 7),
        new(1, 3, 4, 5, 8),
        new(1, 2, 5, 6, 7),
        new(1, 2, 4, 6, 8),
        new(1, 2, 4, 5, 9),
        new(1, 2, 3, 7, 8),
        new(1, 2, 3, 6, 9),
        new(1, 2, 3, 5, 10),
        new(1, 2, 3, 4, 11),
    };
    
}