using Core.Common;

namespace Main;

public static class TestHelper
{
    public static IEnumerable<Kvp> GetTestList(int count)
    {
        var rand = new Random();
        for (int i = 1; i <= count; i++)
        {
            yield return new(1, "value-" + i, rand.Next(0,5) > 1 ? true : false);
        }
    }
    
    public static void PrintKvp(Kvp kvp) => Console.WriteLine($"{kvp.Key}: {kvp.Value}, IsTombstoned: {kvp.IsTombStoned}");
}