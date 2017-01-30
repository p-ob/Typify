namespace Typify
{
    using System.Reflection;

    internal class TypeScriptProperty
    {
        public PropertyInfo Source { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsNullable { get; set; }

        public bool IsReadonly { get; set; }

        public string ToTypescriptString()
        {
            return $"{(IsReadonly ? "readonly " : "")}{Name}{(IsNullable ? "?" : "")}: {Type};";
        }
    }
}
