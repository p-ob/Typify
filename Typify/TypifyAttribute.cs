namespace Typify
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class TypifyAttribute : Attribute
    {
        public Type Type { get; }

        public TypifyAttribute(Type type)
        {
            Type = type;
        }
    }
}
