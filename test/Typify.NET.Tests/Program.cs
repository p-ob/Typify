//namespace Typify.NET.Tests
//{
//    using System;
//    using System.Reflection;

//    internal class Program
//    {
//        public static void Main(string[] args)
//        {
//            var assembly = Assembly.GetEntryAssembly();
//            Console.WriteLine("Typification commencing.");
//            Typifier.Typify(new TypifyOptions
//            {
//                NamingStrategy = NamingStrategy.CamelCase,
//                AssemblyFile = assembly.Location
//            });
//            Console.WriteLine("Typification complete.");
//            Console.Read();
//        }
//    }
//}