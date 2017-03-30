﻿namespace Typify.NET.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Utils;

    internal class TypeScriptGenericInterfaceDefinition : TypeScriptInterfaceDefinition
    {
        public TypeScriptGenericInterfaceDefinition(Type source, TypifyOptions options) : base(source, options)
        {
        }

        public new static (TypeScriptDefinition definition, IEnumerable<Type> dependentTypes) Create(Type source, TypifyOptions options)
        {
            var definition = new TypeScriptGenericInterfaceDefinition(source, options);
            var dependentTypes =
                definition.Properties.Where(
                        p =>
                            p.IsImport ||
                            !(p.Type.IsSystemType() || TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(p.Type)))
                    .Select(p => p.Type);

            return (definition, dependentTypes);
        }

        public override string ToTypeScriptString(int startTabIndex = 0)
        {
            var tabsString = new string('\t', startTabIndex);
            return
                $"{tabsString}export interface {FormatName()}<{GetGenericArgumentsString()}>{(Base != null ? GetExtends() : string.Empty)} {{\n{tabsString}\t{string.Join($"\n{tabsString}\t", Properties.Select(p => p.ToTypeScriptString()))}\n{tabsString}}}";
        }

        protected override string FormatName()
        {
            return Source.GetNameWithoutGenericArity();
        }

        private string GetGenericArgumentsString()
        {
            return string.Join(",", Source.GetTypeInfo().GetGenericArguments().Select(arg => arg.Name));
        }

        private string GetExtends()
        {
            return $" extends {Base.TypeScriptType}";
        }
    }
}
