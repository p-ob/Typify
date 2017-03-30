namespace Typify.NET
{
    using System.Reflection;
    using Typify.NET.Models;
    using Typify.NET.Utils;

    internal static class TypeScriptPropertyFactory
    {
        public static TypeScriptProperty BuildFromMemberInfo(MemberInfo memberInfo, string sourceNamespace, TypifyOptions typifyOptions)
        {
            var tsProperty = new TypeScriptProperty(memberInfo, typifyOptions);
            tsProperty.IsImport = !string.Equals(tsProperty.Namespace.ToTypeScriptNamespace(), sourceNamespace) &&
                                  !(TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(tsProperty.Type) ||
                                    tsProperty.Type.IsSystemType());

            return tsProperty;
        }
    }
}
