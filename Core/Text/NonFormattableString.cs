namespace Jay.SourceGen.Text;

public readonly struct NonFormattableString : IEquatable<string>
{
    //public static implicit operator string(NonFormattableString nfs) => nfs._str;
    public static implicit operator NonFormattableString(string? str) => new NonFormattableString(str);
    public static implicit operator NonFormattableString(FormattableString _) => throw new InvalidOperationException();
    //public static implicit operator NonFormattableString(ReadOnlySpan<char> _) => throw new InvalidOperationException();

    internal readonly string _str;

    public ReadOnlySpan<char> Text => _str.AsSpan();

    private NonFormattableString(string? str)
    {
        _str = str ?? "";
    }

    public bool Equals(string? text) => string.Equals(_str, text);

    public bool Equals(string? text, StringComparison comparison) => string.Equals(_str, text, comparison);

    public bool Equals(ReadOnlySpan<char> text, StringComparison comparison = StringComparison.Ordinal) => _str.AsSpan().Equals(text, comparison);

    public override bool Equals(object? obj)
    {
        return obj is string text && Equals(text);
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