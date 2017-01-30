namespace Typify.Test
{
    using System;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Typification commencing.");
            Typifier.Typify(new TypifyOptions
            {
                NamingStrategy = NamingStrategy.SnakeCase
            });
            Console.WriteLine("Typification complete.");
            Console.Read();
        }
    }
}