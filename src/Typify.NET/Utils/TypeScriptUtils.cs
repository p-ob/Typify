namespace Typify.NET.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
    }
}
