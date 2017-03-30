namespace Typify.NET
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Typify.NET.Models;

    internal static class TypesScriptDefinitionFactory
    {
        public static (TypeScriptDefinition definition, IEnumerable<Type> dependentTypes) BuildFromType(Type source, TypifyOptions options)
        {
            var results = (definition: (TypeScriptDefinition)null, dependentTypes: Enumerable.Empty<Type>());
            var sourceTypeInfo = source.GetTypeInfo();
            if (sourceTypeInfo.IsEnum)
            {
                results.definition = new TypeScriptEnumDefinition(source, options);
            }
            else
            {
                if (sourceTypeInfo.IsGenericType)
                {
                    results = TypeScriptGenericInterfaceDefinition.Create(source, options);
                }
                else
                {
                    results = TypeScriptInterfaceDefinition.Create(source, options);
                }
            }
            return results;
        }
    }
}
