namespace Typify.Test
{
    using System.Collections.Generic;

    public class SubEntity
    {
        public int Id { get; set; }

        public IEnumerable<int> SomeArray { get; set; }
    }
}
