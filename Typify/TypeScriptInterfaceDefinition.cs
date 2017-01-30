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
            return properties.Select(p => new TypeScriptProperty(p, _options));
        }
    }
}
