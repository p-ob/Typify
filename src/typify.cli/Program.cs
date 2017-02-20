namespace Typify.NET.Cli
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Typify.NET.Cli.Generate;
	using Typify.NET.Cli.Help;
	using Typify.NET.Cli.Utils;

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
				Reporter.Error.WriteLine(e.ToString());
				return 1;
			}
		}

		private static int ProcessArgs(string[] args)
		{
			bool? verbose = null;
			var success = true;
			var command = string.Empty;
			var arg = string.Empty;
			var lastArg = 0;
			var exitCode = 1;

			for (; lastArg < args.Length; lastArg++)
			{
				arg = args[lastArg];
				if (IsArg(arg, "d", "diagnostics"))
				{
					verbose = true;
				}
				else if (IsArg(arg, "h", "help") || arg == "-?" || arg == "/?")
				{
					//HelpCommand.PrintHelp();
					return 0;
				}
				else if (arg.StartsWith("-"))
				{
					Reporter.Error.WriteLine($"Unknown option: {arg}");
					success = false;
				}
				else
				{
					// It's the command, and we're done!
					command = arg;
					break;
				}
			}
			if (!success)
			{
				HelpCommand.Run(Array.Empty<string>());
				return 1;
			}

			var appArgs = (lastArg + 1) >= args.Length ? Enumerable.Empty<string>() : args.Skip(lastArg + 1).ToArray();

			if (verbose.HasValue)
			{
				Environment.SetEnvironmentVariable(Context.Variables.Verbose, verbose.ToString());
			}

			if (string.IsNullOrEmpty(command))
			{
				command = "help";
			}

			if (BuiltIns.TryGetValue(command, out Func<string[], int> builtIn))
			{
				Reporter.Output.WriteLine($"Matched command {command}");
				exitCode = builtIn(appArgs.ToArray());
			}

			Reporter.Output.WriteLine($"Returning exit code {exitCode}");
			return exitCode;
		}

		private static bool IsArg(string candidate, string longName)
		{
			return IsArg(candidate, null, longName);
		}

		private static bool IsArg(string candidate, string shortName, string longName)
		{
			return (shortName != null && candidate.Equals("-" + shortName)) || (longName != null && candidate.Equals("--" + longName));
		}
	}
}