namespace Typify.NET.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeScriptUtils
    {
        public static readonly ILookup<Type, string> DotNetTypeToTypeScriptTypeLookup;

        static TypeScriptUtils()
        {
            var typeScriptTypeToDotNetTypes = new Dictionary<string, IEnumerable<Type>>
            {
                {
                    "number",
                    new[]
                    {
                        typeof(int), typeof(float), typeof(decimal), typeof(long), typeof(byte), typeof(sbyte),
                        typeof(short), typeof(ushort), typeof(uint), typeof(ulong)
                    }
                },
                { "string", new[] { typeof(string), typeof(char) } },
                { "boolean", new[] { typeof(bool) } },
                { "Date", new[] { typeof(DateTime), typeof(DateTimeOffset) } },
                { "any", new[] { typeof(object) } }
            };

            DotNetTypeToTypeScriptTypeLookup =
                typeScriptTypeToDotNetTypes.SelectMany(pair => pair.Value, (pair, value) => new { pair.Key, value })
                    .ToLookup(pair => pair.value, pair => pair.Key);
        }

        public static string ToTypeScriptNamespace(this string assemblyNamespace)
        {
            return assemblyNamespace.Replace('.', '-').ToLowerInvariant();
        }

        public static string ToTypeScriptType(this Type type, string targetTsVersion)
        {
            try
            {
                // handle defined mappings
                if (DotNetTypeToTypeScriptTypeLookup.Contains(type))
                {
                    return DotNetTypeToTypeScriptTypeLookup[type].First();
                }

                var typeInfo = type.GetTypeInfo();

                // Dictionaries => map or Object
                if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type))
                {
                    var numberTypes =
                        DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("number"))
                            .Select(t => t.Key);
                    var stringTypes =
                        DotNetTypeToTypeScriptTypeLookup.Where(t => t.Contains("string"))
                            .Select(t => t.Key);
                    var underlyingTypes = typeInfo.GenericTypeArguments;
                    if (underlyingTypes != null && underlyingTypes.Length == 2)
                    {
                        var keyType = underlyingTypes[0];
                        var valueType = underlyingTypes[1];
                        if (numberTypes.Contains(keyType) || stringTypes.Contains(keyType))
                        {
                            var typeScriptKeyType = ToTypeScriptType(keyType, targetTsVersion);
                            var typeScriptValueType = ToTypeScriptType(valueType, targetTsVersion);
                            return $"{{ [key: {typeScriptKeyType}]: {typeScriptValueType}; }}";
                        }
                        var targetVersion = new Version(targetTsVersion);
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
                        return $"{ToTypeScriptType(underlyingTypes.First(), targetTsVersion)}[]";
                    }
                }

                // Nullable types => {type}?
                if (type.IsNullable())
                {
                    return ToTypeScriptType(typeInfo.GenericTypeArguments[0], targetTsVersion);
                }

                if (typeInfo.IsGenericType)
                {
                    var name = type.GetNameWithoutGenericArity();
                    return $"{name}<{string.Join(",", type.GetTypeInfo().GetGenericArguments().Select(args => ToTypeScriptType(args, targetTsVersion)))}>";
                }

                // assume complex objects have TypeScript definitions created
                if (typeInfo.IsClass || typeInfo.IsEnum)
                {
                    //IsImport = Source.DeclaringType.Namespace != Type.Namespace && !Type.IsSystemType();

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
