namespace Jay.SourceGen;

public enum Naming
{
    /// <summary>
    /// No change: "MemberName" => "MemberName"
    /// </summary>
    Default = 0,
    /// <summary>
    /// Field naming: "MemberName" => "_memberName"
    /// </summary>
    Field = 1,
    /// <summary>
    /// Camel-cased naming: "MemberName" => "memberName"
    /// </summary>
    Camel = 2,
    /// <summary>
    /// Pascal-cased naming "memberName" => "MemberName"
    /// </summary>
    Pascal = 3,
}