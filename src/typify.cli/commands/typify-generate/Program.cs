namespace Typify.NET.Cli.Generate
{
	using System;
	using System.Collections.Generic;
    using Microsoft.Extensions.CommandLineUtils;

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

            var namingStrategyOption = app.Option($"--naming-strategy <{LocalizableStrings.ProjectArgumentName}>", LocalizableStrings.ProjectArgumentDescription, CommandOptionType.SingleValue);
            var destinationOption = app.Option($"-d|--destination {LocalizableStrings.DestinationOptionName}", LocalizableStrings.DestinationOptionDescription, CommandOptionType.SingleValue);
			var oneFilePerNamespaceOption = app.Option("--multiple-files", LocalizableStrings.MultipleFilesOptionDescription, CommandOptionType.NoValue);

			app.OnExecute(() =>
            {
                var generateArgs = new List<string>();

				if (!string.IsNullOrEmpty(projectArgument.Value))
                {

                }

                if (namingStrategyOption.HasValue())
                {

                }

                if (oneFilePerNamespaceOption.HasValue())
                {

                }

                if (destinationOption.HasValue())
                {

                }

                return -1;
            });

            return app.Execute(args);
        }
    }
}
