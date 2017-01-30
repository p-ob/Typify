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
                { "Date", new[] { typeof(DateTime), typeof(DateTimeOffset) } },
                { "any", new[] { typeof(object) } }
            };

            DotNetTypeToTypeScriptTypeLookup =
                typeScriptTypeToDotNetTypes.SelectMany(pair => pair.Value, (pair, value) => new { pair.Key, value })
                    .ToLookup(pair => pair.value, pair => pair.Key);
        }

        public static void Typify(TypifyOptions options)
        {
            var typesToTypify = GetTypesToTypify();
            var typeScriptDefinitions = new List<ITypeScriptDefinition>();

            foreach (var type in typesToTypify)
            {
                var definition = GenerateTypeScriptDefinition(type, options.NamingStrategy);
                if (
                    !typeScriptDefinitions.Any(
                        td => td.Name == definition.Namespace && td.Namespace == definition.Namespace))
                {
                    typeScriptDefinitions.Add(definition);
                }
            }

            var namespacedDefinitions = typeScriptDefinitions.GroupBy(td => td.Namespace);
            WriteDefinitions(namespacedDefinitions, options.Destination);
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

        private static ITypeScriptDefinition GenerateTypeScriptDefinition(Type type, NamingStrategy namingStrategy)
        {
            ITypeScriptDefinition typeScriptDefinition;
            if (type.GetTypeInfo().IsEnum)
            {
                typeScriptDefinition = Activator.CreateInstance(typeof(TypeScriptEnumDefinition<>).MakeGenericType(type)) as ITypeScriptDefinition;
            }
            else
            {
                typeScriptDefinition = new TypeScriptInterfaceDefinition
                {
                    Source = type,
                    Name = type.Name,
                    Namespace = type.Namespace.Replace('.', '-').ToLowerInvariant(),
                    Properties =
                        type.GetProperties(PropertyBindingFlags)
                            .Select(p => MapPropertyInfoToTypeScriptProperty(p, namingStrategy))
                };
            }

            typeScriptDefinition.Namespace = type.Namespace.Replace('.', '-').ToLowerInvariant();
            return typeScriptDefinition;
        }

        private static TypeScriptProperty MapPropertyInfoToTypeScriptProperty(PropertyInfo property,
            NamingStrategy namingStrategy)
        {
            var typeInfo = property.PropertyType.GetTypeInfo();
            var editableAttribute = typeInfo.GetCustomAttribute<EditableAttribute>();

            string FormatName()
            {
                switch (namingStrategy)
                {
                    case NamingStrategy.CamelCase:
                        return property.Name.ToCamelCase();
                    case NamingStrategy.SnakeCase:
                        return property.Name.ToSnakeCase();
                    case NamingStrategy.None:
                    default:
                        return property.Name;
                }
            }

            return new TypeScriptProperty
            {
                Source = property,
                Name = FormatName(),
                Type = MapTypeToTypeScriptType(property.PropertyType),
                IsNullable = typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>),
                IsReadonly = editableAttribute != null && !editableAttribute.AllowEdit
            };
        }

        private static string MapTypeToTypeScriptType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var numberTypes =
                DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("number")).Select(t => t.Key);
            var stringTypes =
                DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("string")).Select(t => t.Key);
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
                }
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var underlyingTypes = typeInfo.GenericTypeArguments;
                if (underlyingTypes != null && underlyingTypes.Length == 1)
                {
                    return $"{MapTypeToTypeScriptType(underlyingTypes.First())}[]";
                }
            }
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return MapTypeToTypeScriptType(typeInfo.GenericTypeArguments[0]);
            }

            if (DotNetTypeToTypeScriptTypeLookup.Contains(type))
            {
                return DotNetTypeToTypeScriptTypeLookup[type]?.FirstOrDefault();
            }
            if (typeInfo.IsClass)
            {
                return type.Name;
            }

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

        private static void WriteDefinitions(IEnumerable<IGrouping<string, ITypeScriptDefinition>> namespacedDefinitions, string destination)
        {
            var typeScriptFileContents = "/*\n\tAutogenerated using Typify. Do not modify.\n*/\n";
            foreach (var namespacedDefinition in namespacedDefinitions)
            {
                typeScriptFileContents +=
                    $"declare module '{namespacedDefinition.Key}' {{\n{string.Join("\n", namespacedDefinition.Select(d => d.ToTypescriptString(1)))}\n}}";
            }

            var file =
                $"{destination}{(destination.EndsWith("/") || destination == string.Empty ? string.Empty : "/")}typified.d.ts";
            var stream = File.Create(file);
            using (var tw = new StreamWriter(stream))
            {
                tw.Write(typeScriptFileContents);
            }
        }
    }
}
