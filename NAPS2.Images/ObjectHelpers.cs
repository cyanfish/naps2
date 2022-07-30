namespace NAPS2.Images;

public static class ObjectHelpers
{
    public static bool ListEquals<T>(IList<T> first, IList<T> second)
    {
        if (second.Count != first.Count)
        {
            return false;
        }

        for (int i = 0; i < first.Count; i++)
        {
            if (!second[i]!.Equals(first[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static int ListHashCode<T>(IList<T> list)
    {
        unchecked
        {
            return list.Aggregate(0, (hash, item) => (hash * 397) ^ item!.GetHashCode());            
        }
    }
}