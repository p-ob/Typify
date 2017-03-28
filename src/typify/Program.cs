namespace Typify.NET.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Typify.NET.Tools.Generate;
    using Typify.NET.Tools.Help;

    public static class Program
    {
        private static readonly Dictionary<string, Func<string[], int>> BuiltIns = new Dictionary<string, Func<string[], int>>
        {
            ["generate"] = GenerateCommand.Run
        };

        private static readonly AppArg VerboseArg;
        private static readonly AppArg HelpArg;

        static Program()
        {
            VerboseArg = new AppArg("diagnostics", "d");
            HelpArg = new AppArg("help", "h");
        }


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
            var exitCode = 0;

            for (; lastArg < args.Length; lastArg++)
            {
                arg = args[lastArg];
                if (VerboseArg.IsArg(arg))
                {
                    verbose = true;
                }
                else if (HelpArg.IsArg(arg) || arg == "-?" || arg == "/?")
                {
                    HelpCommand.Run(Array.Empty<string>());
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
                exitCode = 1;
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

            if (exitCode == 0 && BuiltIns.TryGetValue(command, out Func<string[], int> builtIn))
            {
                try
                {
                    exitCode = builtIn(appArgs.ToArray());
                }
                catch (TypifyException e)
                {
                    if (verbose.GetValueOrDefault())
                    {
                        Reporter.Error.WriteLine(e.ToString());
                    }
                    exitCode = 1;
                }
            }

            return exitCode;
        }
    }
}