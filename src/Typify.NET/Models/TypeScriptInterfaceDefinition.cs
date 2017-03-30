namespace Typify.NET.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utils;

    internal class TypeScriptInterfaceDefinition : TypeScriptDefinition
    {
        public IEnumerable<TypeScriptProperty> Properties { get; }

        public TypeScriptBaseClass Base { get; }

        public TypeScriptInterfaceDefinition(Type source, TypifyOptions options) : base(source, options)
        {
            Base = TypeScriptBaseClassFactory.BuildFromType(source.GetTypeInfo().BaseType, Namespace);
            Properties = GetTypeScriptProperties();
        }

        public static (TypeScriptDefinition definition, IEnumerable<Type> dependentTypes) Create(Type source, TypifyOptions options)
        {
            var definition = new TypeScriptInterfaceDefinition(source, options);
            var dependentTypes =
                definition.Properties.Where(
                        p =>
                            p.IsImport ||
                            !(p.Type.IsSystemType() || TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(p.Type)))
                    .Select(p => p.Type).ToList();

            if (definition.Base != null)
            {
                dependentTypes.Add(definition.Base.Type);
            }

            dependentTypes = dependentTypes.Distinct().ToList();

            return (definition, dependentTypes);
        }

        public override string ToTypeScriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {SourceName}{(Base != null ? GetExtendsTypeScriptString() : string.Empty)} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypeScriptString()))}\n{tabsString}}}";
        }

        protected IEnumerable<TypeScriptProperty> GetTypeScriptProperties()
        {
            var typeInfo = Source.GetTypeInfo();
            var properties = typeInfo.GetProperties(TypeUtils.MemberBindingFlags).Distinct();
            var fields = typeInfo.GetFields(TypeUtils.MemberBindingFlags);
            return
                properties.Concat<MemberInfo>(fields)
                    .Select(m => TypeScriptPropertyFactory.BuildFromMemberInfo(m, Namespace, Options));
        }

        private string GetExtendsTypeScriptString()
        {
            return $" extends {Base.TypeScriptType}";
        }
    }
}
