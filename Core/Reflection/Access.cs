namespace Jay.SourceGen.Reflection;

[Flags]
public enum Access
{
    None = 0,
    Instance = 1 << 0,
    Static = 1 << 1,
}