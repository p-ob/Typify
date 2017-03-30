namespace Typify.NET.Models
{
    using System;
    using Typify.NET.Utils;

    internal abstract class TypeScriptDefinition
    {
        protected TypifyOptions Options { get; }

        public Type Source { get; }

        public string SourceName { get; }

        public string Namespace { get; }

        public abstract string ToTypeScriptString(int startTabIndex = 0);

        protected virtual string FormatName()
        {
            return SourceName;
        }

        protected TypeScriptDefinition(Type type, TypifyOptions options)
        {
            Source = type;
            SourceName = type.Name;
            Namespace = type.Namespace.ToTypeScriptNamespace();
            Options = options;
        }
    }
}
