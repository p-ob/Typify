namespace Typify.NET.Tests
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Entity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        public int? NullableInt { get; set; }

        [Editable(false)]
        public int ReadonlyInt { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public float? ReadonlyNullableFloat { get; private set; }

        public SubEntity SubEntity { get; set; }

        public GenericClass<int, string> GenericClass { get; set; }

        private int _notPublic;
    }
}
