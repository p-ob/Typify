namespace Typify.NET.Tests.Library
{
    using System.Collections.Generic;
    using Some.Other.Namespace;

    public class SubEntity
    {
        public int Id { get; set; }

        public IEnumerable<int> SomeArray { get; set; }

        public SomeEnum SomeEnum { get; set; }

        public DifferentNamespaceClass ExternalReference { get; set; }
    }
}
