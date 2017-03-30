namespace Typify.NET
{
    using System;
    using Typify.NET.Models;
    using Typify.NET.Utils;

    internal static class TypeScriptBaseClassFactory
    {
        public static TypeScriptBaseClass BuildFromType(Type baseType, string superTypeNamespace)
        {
            if (
                !(baseType == null || TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(baseType) ||
                  baseType.IsSystemType()))
            {
                var baseClass = new TypeScriptBaseClass
                {
                    TypeScriptType = baseType.Name,
                    Namespace = baseType.Namespace.ToTypeScriptNamespace()
                };

                baseClass.IsImport = !string.Equals(baseClass.Namespace, superTypeNamespace);
                return baseClass;
            }

            return null;
        }
    }
}
