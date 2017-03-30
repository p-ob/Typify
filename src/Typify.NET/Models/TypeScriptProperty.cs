namespace Typify.NET.Models
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utils;

    internal class TypeScriptProperty : TypeScriptImportable
    {
        private readonly TypifyOptions _options;

        public MemberInfo Source { get; }

        public Type Type { get; }

        public string Name { get; }

        public bool IsNullable { get; }

        public bool IsReadonly { get; }

        public TypeScriptProperty(MemberInfo source, TypifyOptions options)
        {
            _options = options;
            Source = source;
            Type memberType;
            var propertyNoSetMethod = false;
            switch (source)
            {
                case PropertyInfo p:
                    memberType = p.PropertyType;
                    propertyNoSetMethod = p.GetSetMethod() == null;
                    break;
                case FieldInfo f:
                    memberType = f.FieldType;
                    break;
                default:
                    throw new TypifyException(
                        $"Typify tried to process a member that wasn't supported. Member type given: {source.GetType()}");
            }
            Type = memberType;
            Name = FormatName();
            Namespace = Type.Namespace.ToTypeScriptNamespace();
            TypeScriptType = Type.ToTypeScriptType(_options.TargetTypeScriptVersion);
            IsNullable = Type.IsNullable();
            var editableAttribute = source.GetCustomAttribute<EditableAttribute>();
            IsReadonly = (editableAttribute != null && !editableAttribute.AllowEdit) || propertyNoSetMethod;
        }

        public string ToTypeScriptString()
        {
            return $"{(IsReadonly ? "readonly " : "")}{Name}{(IsNullable ? "?" : "")}: {TypeScriptType};";
        }

        private string FormatName()
        {
            switch (_options.NamingStrategy)
            {
                case NamingStrategy.CamelCase:
                    return Source.Name.ToCamelCase();
                case NamingStrategy.SnakeCase:
                    return Source.Name.ToSnakeCase();
                case NamingStrategy.None:
                default:
                    return Source.Name;
            }
        }
    }
}
