﻿namespace Typify.NET.Tests.Library
{
    using System.Collections.Generic;

    public class GenericClass<T, T2>
    {
        public T GenericProperty { get; set; }

        public IEnumerable<T2> GenericProperties2 { get; set; }

        public Dictionary<string, int> DictionaryMap { get; set; }

        public Dictionary<object, int> DictionaryObject { get; set; }
    }
}
