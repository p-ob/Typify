namespace Typify.NET
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
            var propertyNoSetMethod = false;
            switch (Source)
            {
                case PropertyInfo p:
                    Type = p.PropertyType;
                    propertyNoSetMethod = p.GetSetMethod() == null;
                    break;
                case FieldInfo f:
                    Type = f.FieldType;
                    break;
                default:
                    throw new TypifyException(
                        $"Typify tried to process a member that wasn't supported. Member type given: {source.GetType()}");
            }
            Name = FormatName();
            Namespace = Type.Namespace.ToTypeScriptNamespace();
            TypeScriptType = MapTypeToTypeScriptType(Type);
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

        private string MapTypeToTypeScriptType(Type type)
        {
            try
            {
                // handle defined mappings
                if (TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(type))
                {
                    return TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup[type].First();
                }

                var typeInfo = type.GetTypeInfo();
                
                // Dictionaries => map or Object
                if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type))
                {
                    var numberTypes =
                        TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("number"))
                            .Select(t => t.Key);
                    var stringTypes =
                        TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("string"))
                            .Select(t => t.Key);
                    var underlyingTypes = typeInfo.GenericTypeArguments;
                    if (underlyingTypes != null && underlyingTypes.Length == 2)
                    {
                        var keyType = underlyingTypes[0];
                        var valueType = underlyingTypes[1];
                        if (numberTypes.Contains(keyType) || stringTypes.Contains(keyType))
                        {
                            var typeScriptKeyType = MapTypeToTypeScriptType(keyType);
                            var typeScriptValueType = MapTypeToTypeScriptType(valueType);
                            return $"{{ [key: {typeScriptKeyType}]: {typeScriptValueType}; }}";
                        }
                        var targetVersion = new Version(_options.TargetTypeScriptVersion);
                        var ts22Version = new Version(2, 2);

                        // support 2.2 "object" type
                        return targetVersion.CompareTo(ts22Version) >= 0 ? "object" : "any";
                    }
                }

                // IEnumerable => []{type}
                if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
                {
                    var underlyingTypes = typeInfo.GenericTypeArguments;
                    if (underlyingTypes != null && underlyingTypes.Length == 1)
                    {
                        return $"{MapTypeToTypeScriptType(underlyingTypes.First())}[]";
                    }
                }

                // Nullable types => {type}?
                if (type.IsNullable())
                {
                    return MapTypeToTypeScriptType(typeInfo.GenericTypeArguments[0]);
                }

                if (typeInfo.IsGenericType)
                {
                    var name = type.GetNameWithoutGenericArity();
                    return $"{name}<{string.Join(",", type.GetTypeInfo().GetGenericArguments().Select(MapTypeToTypeScriptType))}>";
                }

                // assume complex objects have TypeScript definitions created
                if (typeInfo.IsClass || typeInfo.IsEnum)
                {
                    IsImport = Source.DeclaringType.Namespace != Type.Namespace && !Type.IsSystemType();

                    return type.Name;
                }

                return "any";
            }
            // TODO log exception with appropriate information
            catch (Exception)
            {
                return "any";
            }
        }
    }
}
