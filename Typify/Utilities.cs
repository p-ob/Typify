namespace Typify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal static class Utilities
    {
        public const BindingFlags PropertyBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static readonly ILookup<Type, string> DotNetTypeToTypeScriptTypeLookup;

        static Utilities()
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

        public static string ToCamelCase(this string str)
        {
            if (str == null || str.Length < 2)
                return str;

            var words = str.SplitOnCapitalLetters();
            var result = words[0].ToLower();
            for (var i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }

        public static string ToSnakeCase(this string str)
        {
            if (str == null || str.Length < 2)
                return str;

            var words = str.SplitOnCapitalLetters();
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = words[i].ToLowerInvariant();
            }
            var result = string.Join("_", words);

            return result;
        }

        public static bool IsNullable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static string ToTypeScriptNamespace(this string assemblyNamespace)
        {
            return assemblyNamespace.Replace('.', '-').ToLowerInvariant();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        private static string[] SplitOnCapitalLetters(this string s)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Split(s);
        }
    }
}
