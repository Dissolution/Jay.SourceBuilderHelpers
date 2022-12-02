namespace Jay.SourceGen;

internal static class Constants
{
    public static readonly int Pool_MinCapacity = Environment.ProcessorCount * 2;
    public const int Pool_MaxCapacity = 0X7FEFFFFF; // = Array.MaxLength
}