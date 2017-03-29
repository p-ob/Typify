namespace Typify.NET.Tests.Lab
{
    using System;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Typify beginning...");
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = typeof(Library.Entity).GetTypeInfo().Assembly.Location
            };
            Typifier.Typify(typifyOptions);
            Console.WriteLine("Typify ended.\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}