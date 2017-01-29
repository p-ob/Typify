﻿namespace Typify.Test
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

        public SubEntity SubEntity { get; set; }

        private int _notPublic;
    }
}
