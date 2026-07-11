namespace Main.Helpers;

public static class LinqHelper
{
    public static IEnumerable<T> Choose<T>(IEnumerable<T> list, int elementsCount)
    {
        ReadOnlySpan<T> span = list.ToArray();
        return Random.Shared.GetItems(span, elementsCount);
    }
}