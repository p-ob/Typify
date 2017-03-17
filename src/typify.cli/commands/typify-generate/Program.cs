namespace Typify.NET.Tools.Generate
{
    using System;
    using System.IO;
    using Microsoft.Extensions.CommandLineUtils;
    using Typify.NET.Tools.Utils;

    internal class GenerateCommand
    {
        public static int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "typify generate",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription
            };
            app.HelpOption("-h|--help");

            var projectArgument = app.Argument($"<{LocalizableStrings.ProjectArgumentName}>",
                LocalizableStrings.ProjectArgumentDescription);

	        var namingStrategyOption = app.Option($"--naming-strategy <{LocalizableStrings.NamingStrategyOptionName}>",
		        LocalizableStrings.NamingStrategyOptionDescription, CommandOptionType.SingleValue);
	        var destinationOption = app.Option($"-d|--destination <{LocalizableStrings.DestinationOptionName}>",
		        LocalizableStrings.DestinationOptionDescription, CommandOptionType.SingleValue);
	        var oneFilePerNamespaceOption = app.Option($"--multiple-files <{LocalizableStrings.MultipleFilesOptionName}>",
		        LocalizableStrings.MultipleFilesOptionDescription, CommandOptionType.NoValue);

			app.OnExecute(() =>
            {
                var typifyOptions = new TypifyOptions();

	            var fileOrDir = !string.IsNullOrEmpty(projectArgument.Value)
		            ? projectArgument.Value
		            : Directory.GetCurrentDirectory();

	            var project = MSBuildHelper.LoadProject(fileOrDir);
				typifyOptions.AssemblyName = MSBuildHelper.GetAssemblyName(project);

                if (namingStrategyOption.HasValue())
                {
	                typifyOptions.NamingStrategy = MapStringToNamingStrategyOption(namingStrategyOption.Value());
                }

                if (oneFilePerNamespaceOption.HasValue())
                {
	                typifyOptions.OneFilePerNamespace = true;
                }

                if (destinationOption.HasValue())
                {
	                typifyOptions.Destination = destinationOption.Value();
                }

                return -1;
            });

			Reporter.Output.WriteLine("Executing Generate app");
			return app.Execute(args);
        }

	    private static NamingStrategy MapStringToNamingStrategyOption(string namingStrategyOptionStr)
	    {
		    switch (namingStrategyOptionStr)
		    {
				case "camel":
				case "camelcase":
					return NamingStrategy.CamelCase;
				case "snake":
				case "snakecase":
					return NamingStrategy.SnakeCase;
				case "none":
				    return NamingStrategy.None;
				default:
					throw new Exception("Not a valid naming strategy value.");
		    }
	    }
    }
}
