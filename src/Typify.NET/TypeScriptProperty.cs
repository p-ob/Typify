namespace Typify.NET
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utilities;

    internal class TypeScriptProperty
    {
        private readonly TypifyOptions _options;

        public PropertyInfo Source { get; }

        public string Name => FormatName();

        public bool IsImport { get; private set; }

        public string Namespace => Source.PropertyType.Namespace.ToTypeScriptNamespace();

        public string Type { get; }

        public bool IsNullable => Source.PropertyType.IsNullable();

        public bool IsReadonly
        {
            get
            {
                var editableAttribute = Source.GetCustomAttribute<EditableAttribute>();
                return (editableAttribute != null && !editableAttribute.AllowEdit) || Source.GetSetMethod() == null ||
                       !Source.CanWrite;
            }
        }

        public TypeScriptProperty(PropertyInfo source, TypifyOptions options)
        {
            Source = source;
            Type = MapTypeToTypeScriptType(Source.PropertyType);
            _options = options;
        }

        public string ToTypeScriptString()
        {
            return $"{(IsReadonly ? "readonly " : "")}{Name}{(IsNullable ? "?" : "")}: {Type};";
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
                var numberTypes =
                    TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("number")).Select(t => t.Key);
                var stringTypes =
                    TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("string")).Select(t => t.Key);

                // Dictionaries => map or Object
                if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type))
                {
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
                        return "Object";
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
                    IsImport = Source.DeclaringType.Namespace != Source.PropertyType.Namespace &&
                               !Source.PropertyType.IsSystemType();
                               
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
