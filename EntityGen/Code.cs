﻿//namespace Jay.SourceGen.EntityGen;

///*internal static class CodeIMPL
//{
//    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
//    public sealed class EntityAttribute : Attribute
//    {
//        public bool Nullability { get; set; } = true;

//        public EntityAttribute()
//        {
//        }
//    }


//    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
//    public sealed class KeyAttribute : Attribute
//    {
//        public bool OptOut { get; set; } = false;

//        public KeyAttribute()
//        {
//        }

//        public KeyAttribute(bool optOut)
//        {
//            this.OptOut = optOut;
//        }
//    }

//    // <auto-generated/>
//    [Entity]
//    public partial class EntityBase //: IEquatable<EntityBase>
//    {
//        [Key]
//        public int Id { get; set; }

//        //[Key(true)]
//        public string Name { get; set; }

//        public Guid Guid { get; set; } = Guid.NewGuid();
//    }


//    partial class EntityBase : IEquatable<EntityBase>, IComparable<EntityBase>
//    {
//        public static bool operator ==(EntityBase left, EntityBase right) => 
//            EqualityComparer<int>.Default.Equals(left.Id, right.Id);

//        public static bool operator !=(EntityBase left, EntityBase right) =>
//            !EqualityComparer<int>.Default.Equals(left.Id, right.Id);

//        public static bool operator <(EntityBase left, EntityBase right) => 
//            Comparer<int>.Default.Compare(left.Id, right.Id) < 0;
    
//        public static bool operator <=(EntityBase left, EntityBase right) => 
//            Comparer<int>.Default.Compare(left.Id, right.Id) <= 0;
    
//        public static bool operator >(EntityBase left, EntityBase right) => 
//            Comparer<int>.Default.Compare(left.Id, right.Id) > 0;
    
//        public static bool operator >=(EntityBase left, EntityBase right) => 
//            Comparer<int>.Default.Compare(left.Id, right.Id) >= 0;
    
    
//        public int CompareTo(EntityBase? entityBase)
//        {
//            // Nulls sort first
//            if (entityBase == null) return 1;
//            return Comparer<int>.Default.Compare(this.Id, entityBase.Id);
//        }
    

//        public bool Equals(EntityBase? entityBase)
//        {
//            return entityBase is not null &&
//                EqualityComparer<int>.Default.Equals(this.Id, entityBase.Id);
//        }

//        public override bool Equals(object? obj)
//        {
//            return obj is EntityBase entityBase &&
//                EqualityComparer<int>.Default.Equals(this.Id, entityBase.Id);
//        }

//        public override int GetHashCode()
//        {
        
//            return this.Id.GetHashCode();
//        }

//        public override string ToString()
//        {
//            return $"EntityBase: Id = {this.Id}";
//        }
//    }

//}*/

//internal static partial class Code
//{
//    public static class EntityAttribute
//    {
//        public const string BaseName = "Entity";

//        public const string Name = BaseName + "Attribute";

//        public const string Code = $$"""
//            namespace Jay.SourceGen.EntityGen;

//            [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
//            public sealed class {{Name}} : Attribute
//            {
//                public bool Nullability { get; set; } = true;

//                public {{Name}}()
//                {
                    
//                }
//            }
//            """;
//    }

//    public static class PropertyAttributes
//    {
//        public const string KeyName = "KeyAttribute";
//        public const string KeyNameShort = "Key";
//        public const string EnumName = KeyNameShort + "Kind";
                       
//        public const string Code = $$"""
//            namespace Jay.SourceGen.EntityGen;

//            [Flags]
//            public enum {{EnumName}}
//            {
//                None = 0,
//                Equality   = 1 << 0,
//                Comparison = 1 << 1,
//                Display    = 1 << 2,

//                Id = Equality | Comparison | Display,
//            }


//            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
//            public sealed class {{KeyName}} : Attribute
//            {
//                public {{EnumName}} Kind { get; set; }

//                public KeyAttribute({{EnumName}} kind = {{EnumName}}.Id)
//                {
//                    this.Kind = kind;
//                }
//            }          
//            """;
//    }
//}