namespace Typify.NET.Cli.Utils
{
    internal static class Context
    {
		public const string MSBUILD_SDKS_PATH = "MSBuildSDKsPath";

		public static class Variables
		{
			private const string Prefix = "TYPIFY_CLI_CONTEXT_";
			public static readonly string Verbose = Prefix + "VERBOSE";
		}
	}
}
