namespace Typify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class TypeScriptInterfaceDefinition : ITypeScriptDefinition
    {
        public Type Source { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public IEnumerable<TypeScriptProperty> Properties { get; set; }

        public IEnumerable<ITypeScriptDefinition> Dependencies { get; set; }
        public string ToTypescriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {Name} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypescriptString()))}\n{tabsString}}}";
        }
    }
}
