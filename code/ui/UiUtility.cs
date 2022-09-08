using Sandbox;

namespace Garryware.UI;

public static class UiUtility
{
    
    public static readonly string[] PlaceEmojis = { "🥇", "🥈", "🥉" };

    public static string GetEmojiForPlace(int place)
    {
        int index = place - 1;
        return index >= 0 && index < PlaceEmojis.Length ? PlaceEmojis[index] : string.Empty;
    }

    public static string GetEmojiForLockedInResult(Client client)
    {
        if (client.Pawn is GarrywarePlayer player && player.HasLockedInResult)
        {
            return player.HasWonRound ? "✔" : "❌";
        }
        return string.Empty;
    }

    // @localization
    public static string GetPlaceQualifier(int place)
    {
        int singleDigit = place % 10;
        return singleDigit switch
        {
            1 when place is < 10 or > 20 => "1st",
            2 when place is < 10 or > 20 => "2nd",
            3 when place is < 10 or > 20 => "3rd",
            1 or 2 or 3 => $"{place}th", // 11, 12, 13
            _ => $"{place}th"
        };
    }

}