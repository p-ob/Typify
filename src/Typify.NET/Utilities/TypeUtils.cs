namespace Typify.NET.Utilities
{
	using System;
	using System.Reflection;

	internal static class TypeUtils
	{
		public const BindingFlags PropertyBindingFlags =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

		public static string GetNameWithoutGenericArity(this Type t)
		{
			var name = t.Name;
			var index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}

		public static bool IsSystemType(this Type t)
		{
			return t.Namespace == "System" || t.Namespace.StartsWith("System.");
		}

		public static bool IsNullable(this Type type)
		{
			var typeInfo = type.GetTypeInfo();
			return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
	}
}
