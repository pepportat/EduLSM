namespace Core.Common;

public static class FileNameHelpers
{
    public static int GetSsTableFileTier(string fileName)
    {
        if (fileName.StartsWith("t1"))
        {
            return 1;
        }

        if (fileName.StartsWith("t2"))
        {
            return 2;
        }

        return 3;
    }
}