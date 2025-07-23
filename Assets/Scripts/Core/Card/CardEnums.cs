public enum CardColor
{
    Red = 0,
    Blue = 1,
    Purple = 2,
    Orange = 3,
    Green = 4,
    Yellow = 5
}

public enum PadType
{
    Normal,
    Exchange
}

public static class CardUtility
{
    public static int GetTotalColors()
    {
        return System.Enum.GetValues(typeof(CardColor)).Length;
    }
    public static string GetCardPoolTag(CardColor color)
    {
        return color.ToString() + "Card";
    }
    public static CardColor GetRandomColor(int maxColorIndex)
    {
        int clampedMax = UnityEngine.Mathf.Clamp(maxColorIndex, 0, GetTotalColors() - 1);
        return (CardColor)UnityEngine.Random.Range(0, clampedMax + 1);
    }
}
