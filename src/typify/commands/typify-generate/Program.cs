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

            var assemblyOption = app.Option($"-a|--assembly <{LocalizableStrings.AssemblyOptionName}>",
                LocalizableStrings.AssemblyOptionDescription, CommandOptionType.SingleValue);

            var namingStrategyOption = app.Option($"--naming-strategy <{LocalizableStrings.NamingStrategyOptionName}>",
                LocalizableStrings.NamingStrategyOptionDescription, CommandOptionType.SingleValue);
            var outputOption = app.Option($"-o|--output <{LocalizableStrings.OutputOptionName}>",
                LocalizableStrings.OutputOptionDescription, CommandOptionType.SingleValue);
            var oneFilePerNamespaceOption = app.Option($"--multiple-files <{LocalizableStrings.MultipleFilesOptionName}>",
                LocalizableStrings.MultipleFilesOptionDescription, CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var typifyOptions = new TypifyOptions();

                if (assemblyOption.HasValue())
                {
                    typifyOptions.AssemblyFile = assemblyOption.Value();
                }
                else
                {
                    throw new Exception("Assembly file required.");
                }

                if (namingStrategyOption.HasValue())
                {
                    typifyOptions.NamingStrategy = MapStringToNamingStrategyOption(namingStrategyOption.Value());
                }

                if (oneFilePerNamespaceOption.HasValue())
                {
                    typifyOptions.OneFilePerNamespace = true;
                }

                if (outputOption.HasValue())
                {
                    typifyOptions.Destination = outputOption.Value();
                }

                Typifier.Typify(typifyOptions);
                return 0;
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
