namespace Jay.SourceBuilderHelpers.Text;

public sealed class CodeWriterOptions
{
    public string Indent { get; set; } = "    "; // 4 spaces

    public string NewLine { get; set; } = Environment.NewLine;

    public bool UseJavaStyleBraces { get; set; } = false;

}