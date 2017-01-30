namespace Typify
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    internal class TypeScriptInterfaceDefinition<T> : ITypeScriptDefinition where T : class
    {
        private readonly TypifyOptions _options;

        public Type Source => typeof(T);

        public string Namespace => Source.Namespace.Replace('.', '-').ToLowerInvariant();

        public string Name => Source.Name;

        public IEnumerable<TypeScriptProperty> Properties => GetTypescriptProperties();

        // TODO
        public IEnumerable<ITypeScriptDefinition> Dependencies { get; set; }

        public TypeScriptInterfaceDefinition(TypifyOptions options)
        {
            _options = options ?? new TypifyOptions();
        }

        public string ToTypescriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {Name} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypescriptString()))}\n{tabsString}}}";
        }

        private IEnumerable<TypeScriptProperty> GetTypescriptProperties()
        {
            var properties = Source.GetProperties(Utilities.PropertyBindingFlags);
            return properties.Select(MapPropertyInfoToTypeScriptProperty);
        }

        private TypeScriptProperty MapPropertyInfoToTypeScriptProperty(PropertyInfo propertyInfo)
        {
            var typeInfo = propertyInfo.PropertyType.GetTypeInfo();
            var editableAttribute = typeInfo.GetCustomAttribute<EditableAttribute>();

            string FormatName()
            {
                switch (_options.NamingStrategy)
                {
                    case NamingStrategy.CamelCase:
                        return propertyInfo.Name.ToCamelCase();
                    case NamingStrategy.SnakeCase:
                        return propertyInfo.Name.ToSnakeCase();
                    case NamingStrategy.None:
                    default:
                        return propertyInfo.Name;
                }
            }

            return new TypeScriptProperty
            {
                Source = propertyInfo,
                Name = FormatName(),
                Type = MapTypeToTypeScriptType(propertyInfo.PropertyType),
                IsNullable = propertyInfo.PropertyType.IsNullable(),
                // a property is "readonly" if it's marked as such with DataAnnotations, or has no public setter
                IsReadonly =
                    (editableAttribute != null && !editableAttribute.AllowEdit) || propertyInfo.GetSetMethod() == null
            };
        }

        private string MapTypeToTypeScriptType(Type type)
        {
            // handle defined mappings
            if (Utilities.DotNetTypeToTypeScriptTypeLookup.Contains(type))
            {
                return Utilities.DotNetTypeToTypeScriptTypeLookup[type]?.FirstOrDefault();
            }

            var typeInfo = type.GetTypeInfo();
            var numberTypes =
                Utilities.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("number")).Select(t => t.Key);
            var stringTypes =
                Utilities.DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("string")).Select(t => t.Key);

            // Dictionaries => map or Object
            if (typeof(IDictionary).IsAssignableFrom(type))
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
                        return $"[{typeScriptKeyType}]: {typeScriptValueType}";
                    }
                    return "Object";
                }
            }

            // IEnumerable => []{type}
            if (typeof(IEnumerable).IsAssignableFrom(type))
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

            // assume complex objects have TypeScript definitions created
            if (typeInfo.IsClass || typeInfo.IsEnum)
            {
                return type.Name;
            }

            return "any";
        }
    }
}
