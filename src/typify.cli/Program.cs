namespace Typify.NET.Cli
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Typify.NET.Cli.Generate;

	public class Program
	{
		private static readonly Dictionary<string, Func<string[], int>> BuiltIns = new Dictionary<string, Func<string[], int>>
		{
			["generate"] = GenerateCommand.Run
		};

		public static int Main(string[] args)
		{
			try
			{
				return ProcessArgs(args);
			}
			catch (Exception e)
			{
				return -1;
			}
		}

		internal static int ProcessArgs(string[] args)
		{
			bool? verbose = null;
			var success = true;
			var command = string.Empty;
			var lastArg = 0;
			var exitCode = 1;

			for (; lastArg < args.Length; lastArg++)
			{
				if (IsArg(args[lastArg], "d", "diagnostics"))
				{
					verbose = true;
				}
				else if (IsArg(args[lastArg], "version"))
				{
					//PrintVersion();
					return 0;
				}
				else if (IsArg(args[lastArg], "info"))
				{
					//PrintInfo();
					return 0;
				}
				else if (IsArg(args[lastArg], "h", "help") ||
					args[lastArg] == "-?" ||
					args[lastArg] == "/?")
				{
					//HelpCommand.PrintHelp();
					return 0;
				}
				else if (args[lastArg].StartsWith("-"))
				{
					//Reporter.Error.WriteLine($"Unknown option: {args[lastArg]}");
					success = false;
				}
				else
				{
					// It's the command, and we're done!
					command = args[lastArg];
					break;
				}
			}
			//if (!success)
			//{
			//	HelpCommand.PrintHelp();
			//	return 1;
			//}

			var appArgs = (lastArg + 1) >= args.Length ? Enumerable.Empty<string>() : args.Skip(lastArg + 1).ToArray();

			//if (verbose.HasValue)
			//{
			//	Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, verbose.ToString());
			//	Console.WriteLine($"Telemetry is: {( telemetryClient.Enabled ? "Enabled" : "Disabled" )}");
			//}

			if (string.IsNullOrEmpty(command))
			{
				command = "help";
			}

			if (BuiltIns.TryGetValue(command, out Func<string[], int> builtIn))
			{
				exitCode = builtIn(appArgs.ToArray());
			}

			return exitCode;
		}

		private static bool IsArg(string candidate, string longName)
		{
			return IsArg(candidate, shortName: null, longName: longName);
		}

		private static bool IsArg(string candidate, string shortName, string longName)
		{
			return (shortName != null && candidate.Equals("-" + shortName)) || (longName != null && candidate.Equals("--" + longName));
		}
	}
}