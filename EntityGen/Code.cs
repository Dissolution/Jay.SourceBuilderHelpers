﻿namespace Jay.SourceGen.EntityGen;

/*internal static class CodeIMPL
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class EntityAttribute : Attribute
    {
        public bool Nullability { get; set; } = true;

        public EntityAttribute()
        {
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class KeyAttribute : Attribute
    {
        public bool OptOut { get; set; } = false;

        public KeyAttribute()
        {
        }

        public KeyAttribute(bool optOut)
        {
            this.OptOut = optOut;
        }
    }

    // <auto-generated/>
    [Entity]
    public partial class EntityBase //: IEquatable<EntityBase>
    {
        [Key]
        public int Id { get; set; }

        //[Key(true)]
        public string Name { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();
    }


    partial class EntityBase : IEquatable<EntityBase>, IComparable<EntityBase>
    {
        public static bool operator ==(EntityBase left, EntityBase right) => 
            EqualityComparer<int>.Default.Equals(left.Id, right.Id);

        public static bool operator !=(EntityBase left, EntityBase right) =>
            !EqualityComparer<int>.Default.Equals(left.Id, right.Id);

        public static bool operator <(EntityBase left, EntityBase right) => 
            Comparer<int>.Default.Compare(left.Id, right.Id) < 0;
    
        public static bool operator <=(EntityBase left, EntityBase right) => 
            Comparer<int>.Default.Compare(left.Id, right.Id) <= 0;
    
        public static bool operator >(EntityBase left, EntityBase right) => 
            Comparer<int>.Default.Compare(left.Id, right.Id) > 0;
    
        public static bool operator >=(EntityBase left, EntityBase right) => 
            Comparer<int>.Default.Compare(left.Id, right.Id) >= 0;
    
    
        public int CompareTo(EntityBase? entityBase)
        {
            // Nulls sort first
            if (entityBase == null) return 1;
            return Comparer<int>.Default.Compare(this.Id, entityBase.Id);
        }
    

        public bool Equals(EntityBase? entityBase)
        {
            return entityBase is not null &&
                EqualityComparer<int>.Default.Equals(this.Id, entityBase.Id);
        }

        public override bool Equals(object? obj)
        {
            return obj is EntityBase entityBase &&
                EqualityComparer<int>.Default.Equals(this.Id, entityBase.Id);
        }

        public override int GetHashCode()
        {
        
            return this.Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"EntityBase: Id = {this.Id}";
        }
    }

}*/

internal static partial class Code
{
    public static class EntityAttribute
    {
        public const string BaseName = "Entity";

        public const string Name = BaseName + "Attribute";

        public const string Code = $$"""
            [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
            public sealed class {{Name}} : Attribute
            {
                public bool Nullability { get; set; } = true;

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

                public KeyAttribute(bool {{NotImportantPropertyArgName}})
                {
                    this.{{NotImportantPropertyName}} = {{NotImportantPropertyArgName}};
                }
            }
            """;
    }
}