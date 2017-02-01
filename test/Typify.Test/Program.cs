namespace Typify.Test
{
    using System;
    using Typify.NET;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Typification commencing.");
            Typifier.Typify(new TypifyOptions
            {
                NamingStrategy = NamingStrategy.CamelCase
            });
            Console.WriteLine("Typification complete.");
            Console.Read();
        }
    }
}