namespace Typify.NET
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utilities;

    internal class TypeScriptInterfaceDefinition<T> : TypeScriptInterfaceDefinition where T : class
    {
        private readonly TypifyOptions _options;

        public Type Source => typeof(T);

        public override string Namespace => Source.Namespace.ToTypeScriptNamespace();

        public override string Name => Source.Name;

        public override IEnumerable<TypeScriptProperty> Properties => GetTypescriptProperties();

        public override IEnumerable<ITypeScriptDefinition> Dependencies { get; set; }

        public TypeScriptInterfaceDefinition(TypifyOptions options)
        {
            _options = options ?? new TypifyOptions();
        }

        public override string ToTypescriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {Name} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypescriptString()))}\n{tabsString}}}";
        }

        private IEnumerable<TypeScriptProperty> GetTypescriptProperties()
        {
            var properties = Source.GetProperties(TypeUtils.PropertyBindingFlags).Distinct();
            return properties.Select(p => new TypeScriptProperty(p, _options));
        }
    }
}
