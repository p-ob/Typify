namespace Typify.NET.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class TypeScriptEnumDefinition : TypeScriptDefinition
    {
        public IEnumerable<string> EnumValueNames => Enum.GetNames(Source);

        public string Name => Source.Name;

        public TypeScriptEnumDefinition(Type source, TypifyOptions options) : base(source, options)
        {
            
        }

        public override string ToTypeScriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export enum {Name} {{\n{tabsString}\t{string.Join($",\n{tabsString}\t", EnumValueNames.Select(FormatEnumValue))}\n{tabsString}}}";
        }

        private string FormatEnumValue(string enumValueName)
        {
            var value = GetEnumValue(enumValueName);
            return $"{enumValueName} = {value}";
        }

        private int GetEnumValue(string enumValueName)
        {
            return (int)Enum.Parse(Source, enumValueName);
        }
    }
}
