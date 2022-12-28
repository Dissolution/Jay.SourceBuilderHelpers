namespace Jay.EntityGen;

internal static class Code
{
    


    public static class EntityAttribute
    {
        public const string BaseName = "Entity";

        public const string Name = BaseName + "Attribute";

        public const string Code = $$"""
            [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
            public sealed class {{Name}} : Attribute
            {
                public {{Name}}()
                {

                }
            }
            """;
    }

    public static class KeyAttribute
    {
        public const string BaseName = "Key";
        public const string Name = BaseName + "Attribute";

        public const string NotImportantPropertyName = "OptOut";
        public const string NotImportantPropertyArgName = "optOut";

        public const string Code = $$"""
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
            public sealed class KeyAttribute : Attribute
            {
                public bool {{NotImportantPropertyName}} { get; set; } = false;

                public KeyAttribute()
                {

                }

                public KeyAttribute(bool {{NotImportantPropertyArgName}})                {
                    this.{{NotImportantPropertyName}} = {{NotImportantPropertyArgName}};
                }
            }
            """;
    }
   
}