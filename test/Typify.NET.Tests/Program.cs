namespace Typify.Test
{
	using System;
	using System.Reflection;
	using Typify.NET;

	internal class Program
	{
		public static void Main(string[] args)
		{
			var assembly = Assembly.GetEntryAssembly();
			Console.WriteLine("Typification commencing.");
			Typifier.Typify(new TypifyOptions
			{
				NamingStrategy = NamingStrategy.CamelCase,
				AssemblyName = assembly.FullName
			});
			Console.WriteLine("Typification complete.");
			Console.Read();
		}
	}
}