namespace Typify.NET.Tools.Generate
{
	internal static class LocalizableStrings
	{
		public const string AppDescription = "Generate TypeScript definitions for .NET classes marked with TypifyAttribute";

		public const string AppFullName = "Typify.NET Generator";

		public const string ProjectArgumentName = "PROJECT";

		public const string ProjectArgumentDescription = "The source project to Typify.";

		public const string NamingStrategyOptionName = "NAMING-STRATEGY";

		public const string NamingStrategyOptionDescription =
			"Tells Typify how to convert .NET casing; available options are camel(case), snake(case), or none";

		public const string DestinationOptionName = "DESTINATION";

		public const string DestinationOptionDescription = "The output destination for the type declartion file(s)";

		public const string MultipleFilesOptionName = "MULTIPLE-FILES";

		public const string MultipleFilesOptionDescription =
			"Tells Typify to generate one TypeScript declaration file per namespace";
	}
}
