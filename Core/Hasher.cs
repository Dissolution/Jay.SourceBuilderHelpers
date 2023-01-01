using System.Collections.Immutable;

namespace Jay.SourceGen;

public static class Hasher
{
     public static int GenerateHashCode<T>(ImmutableArray<T> array)
    {
        unchecked
        {
            int hash = 1009;
            int count = array.Length;
            for (var i = 0; i < count; i++)
            {
                var itemHash = (array[i]?.GetHashCode() ?? 0);
                hash = (hash * 9176) + itemHash;
            }
            return hash;
        }
    }

    public static int GenerateHashCode<T>(ImmutableArray<T> array, Func<T?, int> getItemHash)
    {
        unchecked
        {
            int hash = 1009;
            int count = array.Length;
            for (var i = 0; i < count; i++)
            {
                var itemHash = getItemHash(array[i]);
                hash = (hash * 9176) + itemHash;
            }
            return hash;
        }
    }

    public static int GenerateHashCode<T>(IReadOnlyList<T> values)
    {
        unchecked
        {
            int hash = 1009;
            int count = values.Count;
            for (var i = 0; i < count; i++)
            {
                var itemHash = (values[i]?.GetHashCode() ?? 0);
                hash = (hash * 9176) + itemHash;
            }
            return hash;
        }
    }

    public static int GenerateHashCode<T>(IEnumerable<T> values)
    {
        unchecked
        {
            int hash = 1009;
            foreach (var value in values)
            {
                var itemHash = (value?.GetHashCode() ?? 0);
                hash = (hash * 9176) + itemHash;
            }          
            return hash;
        }
    }
}

