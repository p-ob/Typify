namespace Typify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class TypeScriptEnumDefinition<T> : ITypeScriptDefinition where T : struct, IConvertible
    {
        public Type Source => typeof(T);

        public string Namespace => Source.Namespace.Replace('.', '-').ToLowerInvariant();

        public IEnumerable<string> EnumValueNames => Enum.GetNames(Source);

        public string Name => Source.Name;

        public string ToTypescriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export enum {Name} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", EnumValueNames.Select(FormatEnumValue))}\n{tabsString}}}";
        }

        private string FormatEnumValue(string enumValueName)
        {
            var value = GetEnumValue(enumValueName);
            return $"{enumValueName} = {value},";
        }

        private object GetEnumValue(string enumValueName)
        {
            return (int)Enum.Parse(Source, enumValueName);
        }
    }
}
