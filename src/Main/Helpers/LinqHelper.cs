namespace Main.Helpers;

public static class LinqHelper
{
    public static IEnumerable<T> Choose<T>(IEnumerable<T> list, int elementsCount)
    {
        return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount);
    }
}