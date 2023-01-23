using System.Collections.Immutable;

namespace Jay.SourceGen;

public static class Hasher
{
    private const int SEED = 1009;
    private const int MULTIPLIER = 9176;

    public static int GenerateHashCode<T>(ImmutableArray<T> array)
    {
        unchecked
        {
            int hash = SEED;
            int count = array.Length;
            for (var i = 0; i < count; i++)
            {
                var itemHash = (array[i]?.GetHashCode() ?? 0);
                hash = (hash * MULTIPLIER) + itemHash;
            }

            return hash;
        }
    }

    public static int GenerateHashCode<T>(ImmutableArray<T> array, Func<T?, int> getItemHash)
    {
        unchecked
        {
            int hash = SEED;
            int count = array.Length;
            for (var i = 0; i < count; i++)
            {
                var itemHash = getItemHash(array[i]);
                hash = (hash * MULTIPLIER) + itemHash;
            }

            return hash;
        }
    }

    public static int GenerateHashCode<T>(IReadOnlyList<T> values)
    {
        unchecked
        {
            int hash = SEED;
            int count = values.Count;
            for (var i = 0; i < count; i++)
            {
                var itemHash = (values[i]?.GetHashCode() ?? 0);
                hash = (hash * MULTIPLIER) + itemHash;
            }

            return hash;
        }
    }

    public static int GenerateHashCode<T>(IEnumerable<T> values)
    {
        unchecked
        {
            int hash = SEED;
            foreach (var value in values)
            {
                var itemHash = (value?.GetHashCode() ?? 0);
                hash = (hash * MULTIPLIER) + itemHash;
            }

            return hash;
        }
    }
}