namespace Typify.NET.Models
{
    using System;

    public class TypeScriptImportable
    {
        public Type Type { get; set; }

        public string TypeScriptType { get; set; }

        public string Namespace { get; set; }

        public bool IsImport { get; set; }
    }
}
