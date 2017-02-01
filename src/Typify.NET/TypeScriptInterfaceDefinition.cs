namespace Typify.NET
{
    using System;
    using System.Collections.Generic;

    internal abstract class TypeScriptInterfaceDefinition : ITypeScriptDefinition
    {
        public virtual string Name { get; set; }
        public virtual string Namespace { get; set; }
        public virtual IEnumerable<TypeScriptProperty> Properties { get; set; }
        public virtual IEnumerable<ITypeScriptDefinition> Dependencies { get; set; }
        public virtual string ToTypescriptString(int startTabIndex = 0)
        {
            throw new NotImplementedException();
        }
    }
}
