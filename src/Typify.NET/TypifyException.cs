namespace Typify.NET
{
    using System;

    public class TypifyException : Exception
    {
        public TypifyException(string message) : base(message)
        {
            
        }

        public TypifyException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}
