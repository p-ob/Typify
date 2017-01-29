namespace Typify
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class Typifier
    {
        private const BindingFlags PropertyBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        private static readonly ILookup<Type, string> DotNetTypeToTypeScriptTypeLookup;

        static Typifier()
        {
            var typeScriptTypeToDotNetTypes = new Dictionary<string, IEnumerable<Type>>
            {
                { "number", new[] { typeof(int), typeof(float), typeof(decimal), typeof(long) } },
                { "string", new[] { typeof(string), typeof(char) } },
                { "boolean", new[] { typeof(bool) } },
                { "Date", new[] { typeof(DateTime), typeof(DateTimeOffset) } }
            };

            DotNetTypeToTypeScriptTypeLookup =
                typeScriptTypeToDotNetTypes.SelectMany(pair => pair.Value, (pair, Value) => new { pair.Key, Value })
                    .ToLookup(pair => pair.Value, pair => pair.Key);
        }

        public static void Typify()
        {
            var typesToTypify = GetTypesToTypify();
            var typeScriptDefinitions = new List<TypeScriptDefinition>();

            foreach (var type in typesToTypify)
            {
                var definition = GenerateTypeScriptDefinition(type);
                if (
                    !typeScriptDefinitions.Any(
                        td => td.Name == definition.Namespace && td.Namespace == definition.Namespace))
                {
                    typeScriptDefinitions.Add(definition);
                }
            }

            var namespacedDefinitions = typeScriptDefinitions.GroupBy(td => td.Namespace);
            WriteDefinitions(namespacedDefinitions);
        }

        private static IEnumerable<Type> GetTypesToTypify()
        {
            var typesToTypify = new List<Type>();
            var assembly = Assembly.GetEntryAssembly();
            var typeInfos = assembly.GetTypes().Select(t => t.GetTypeInfo());
            foreach (var t in typeInfos)
            {
                var attributes = t.GetCustomAttributes<TypifyAttribute>();
                if (attributes != null && attributes.Any())
                {
                    var attributeTypes = attributes.Select(a => a.Type);
                    typesToTypify.AddRange(attributeTypes.Where(at => !typesToTypify.Contains(at)));
                }
            }

            var propertyTypes = typesToTypify.SelectMany(GetPropertyTypes).Distinct().ToList();
            typesToTypify.AddRange(propertyTypes.Where(pt => !typesToTypify.Contains(pt)));

            return typesToTypify;
        }

        private static TypeScriptDefinition GenerateTypeScriptDefinition(Type type)
        {
            var typeScriptDefinition = new TypeScriptDefinition
            {
                Source = type,
                Name = type.Name,
                Namespace = type.Namespace, // TODO convert Namespace.Format -> 'namespace-format'
                Properties =
                    type.GetProperties(PropertyBindingFlags)
                        .Select(MapPropertyInfoToTypeScriptProperty)
            };

            return typeScriptDefinition;
        }

        private static TypeScriptProperty MapPropertyInfoToTypeScriptProperty(PropertyInfo property)
        {
            var typeInfo = property.PropertyType.GetTypeInfo();
            var editableAttribute = typeInfo.GetCustomAttribute<EditableAttribute>();
            return new TypeScriptProperty
            {
                Source = property,
                Name = property.Name,
                Type = MapTypeToTypeScriptType(property.PropertyType),
                IsNullable = typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>),
                IsReadonly = editableAttribute != null && !editableAttribute.AllowEdit
            };
        }

        private static string MapTypeToTypeScriptType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var numberTypes = new[] { typeof(int), typeof(float), typeof(decimal), typeof(long) };
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                var underlyingTypes = typeInfo.GenericTypeArguments;
                if (underlyingTypes != null && underlyingTypes.Length == 2)
                {
                    var keyType = underlyingTypes[0];
                    var valueType = underlyingTypes[1];
                    if (numberTypes.Contains(keyType) || typeof(string) == keyType)
                    {
                        var typeScriptKeyType = MapTypeToTypeScriptType(keyType);
                        var typeScriptValueType = MapTypeToTypeScriptType(valueType);
                        return $"[{typeScriptKeyType}]: {typeScriptValueType}";
                    }
                }
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var underlyingTypes = typeInfo.GenericTypeArguments;
                if (underlyingTypes != null && underlyingTypes.Length == 1)
                {
                    return $"[]{MapTypeToTypeScriptType(underlyingTypes.First())}";
                }
            }
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return MapTypeToTypeScriptType(typeInfo.GenericTypeArguments[0]);
            }
            if (numberTypes.Contains(type))
            {
                return "number";
            }
            if (type == typeof(string))
            {
                return "string";
            }
            if (new[] { typeof(DateTime), typeof(DateTimeOffset) }.Contains(type))
            {
                return "Date";
            }
            if (typeInfo.IsClass)
            {
                return type.Name;
            }

            // we can't cover every scenario, return any as a default
            return "any";
        }

        private static IEnumerable<Type> GetPropertyTypes(Type type)
        {
            var propertyTypes =
                type.GetProperties(PropertyBindingFlags)
                    .Where(
                        p =>
                            !(DotNetTypeToTypeScriptTypeLookup.Contains(p.PropertyType) ||
                            p.PropertyType.Namespace == "System" || p.PropertyType.Namespace.StartsWith("System.")))
                    .Select(t => t.PropertyType)
                    .ToList();
            var subPropertyTypes = propertyTypes.SelectMany(GetPropertyTypes).Distinct().ToList();
            propertyTypes.AddRange(subPropertyTypes);
            return propertyTypes;
        }

        private static void WriteDefinitions(IEnumerable<IGrouping<string, TypeScriptDefinition>> namespacedDefinitions)
        {
            var typeScriptFileContents = string.Empty;
            foreach (var namespacedDefinition in namespacedDefinitions)
            {
                var typeScriptNamespace = namespacedDefinition.Key.Replace('.', '-').ToLowerInvariant();
                typeScriptFileContents +=
                    $"declare module \"{typeScriptNamespace}\"{{\n{string.Join("\n", namespacedDefinition.Select(d => d.ToTypescriptString(1)))}\n}}";
            }

            const string file = "typified.d.ts";
            var stream = File.Create(file);
            using (var tw = new StreamWriter(stream))
            {
                tw.Write(typeScriptFileContents);
            }
        }
    }
}
