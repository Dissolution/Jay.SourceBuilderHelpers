namespace Jay.SourceGen.Text;

/// <summary>
/// Provides a way for methods capturing <see cref="FormattableString"/> to exist alongside methods that only care about <see cref="string"/>
/// </summary>
public readonly struct NonFormattableString
{
    public static implicit operator NonFormattableString(string? str) => new NonFormattableString(str);
    public static implicit operator NonFormattableString(FormattableString _) => throw new InvalidOperationException();

    // Stored string, never null
    internal readonly string _str;

    public ReadOnlySpan<char> CharSpan => _str.AsSpan();
    public string Text => _str;

    private NonFormattableString(string? str)
    {
        _str = str ?? "";
    }

    public override bool Equals(object? obj)
    {
        return obj is string text && _str == text;
    }

    public override int GetHashCode()
    {
        return _str.GetHashCode();
    }

    public override string ToString()
    {
        return _str;
    }
}