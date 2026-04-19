using Core.Common;

namespace Main;

public static class TestHelper
{
    public static IEnumerable<Kvp> GetTestList(int count, int startingPoint = 1)
    {
        var rand = new Random();
        for (int i = startingPoint; i < startingPoint + count; i++)
        {
            yield return new(i, "value-" + i, rand.Next(0,5) > 1 ? true : false);
        }
    }
    
    public static void PrintKvp(Kvp kvp) => Console.WriteLine($"{kvp.Key}: {kvp.Value}, IsTombstoned: {kvp.IsTombStoned}");
}