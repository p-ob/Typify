namespace Typify.NET
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utils;

    internal class TypeScriptGenericInterfaceDefinition<T> : TypeScriptInterfaceDefinition where T : class
    {
        private readonly TypifyOptions _options;

        public Type Source => typeof(T).GetGenericTypeDefinition();

        public override string Namespace => Source.Namespace.ToTypeScriptNamespace();

        public override string Name => Source.GetNameWithoutGenericArity();

        public override TypeScriptBaseClass Base => GetBaseType();

        public override IEnumerable<TypeScriptProperty> Properties => GetTypeScriptProperties();

        public TypeScriptGenericInterfaceDefinition(TypifyOptions options)
        {
            _options = options ?? new TypifyOptions();
        }

        public override string ToTypeScriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {Name}<{GetGenericArgumentsString()}>{(Base != null ? GetExtends() : string.Empty)} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypeScriptString()))}\n{tabsString}}}";
        }

        private string GetGenericArgumentsString()
        {
            return string.Join(",", Source.GetTypeInfo().GetGenericArguments().Select(arg => arg.Name));
        }

        private IEnumerable<TypeScriptProperty> GetTypeScriptProperties()
        {
            var typeInfo = Source.GetTypeInfo();
            var properties = typeInfo.GetProperties(TypeUtils.MemberBindingFlags).Distinct();
            var fields = typeInfo.GetFields(TypeUtils.MemberBindingFlags);
            return properties.Concat<MemberInfo>(fields).Select(p => new TypeScriptProperty(p, _options));
        }

        private string GetExtends()
        {
            return $" extends {Base.TypeScriptType}";
        }

        private TypeScriptBaseClass GetBaseType()
        {
            var baseType = Source.GetTypeInfo().BaseType;
            if (
                !(baseType == null || TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(baseType) ||
                  baseType.IsSystemType()))
            {
                var baseClass = new TypeScriptBaseClass
                {
                    TypeScriptType = baseType.Name,
                    Namespace = baseType.Namespace.ToTypeScriptNamespace()
                };

                baseClass.IsImport = !string.Equals(baseClass.Namespace, Namespace);
                return baseClass;
            }

            return null;
        }
    }
}
