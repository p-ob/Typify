namespace Typify.NET.Tools.Generate
{
    using System;
    using Microsoft.Extensions.CommandLineUtils;
    using Typify.NET.Tools.Utils;

    internal static class GenerateCommand
    {
        private static CommandOption _assemblyOption;
        private static CommandOption _namingStrategyOption;
        private static CommandOption _outputOption;
        private static CommandOption _oneFilePerNamespaceOption;

        public static int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "typify generate",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription
            };
            app.HelpOption("-h|--help");

            _assemblyOption = app.Option($"-a|--assembly <{LocalizableStrings.AssemblyOptionName}>",
                LocalizableStrings.AssemblyOptionDescription, CommandOptionType.SingleValue);

            _namingStrategyOption = app.Option($"--naming-strategy <{LocalizableStrings.NamingStrategyOptionName}>",
                LocalizableStrings.NamingStrategyOptionDescription, CommandOptionType.SingleValue);

            _outputOption = app.Option($"-o|--output <{LocalizableStrings.OutputOptionName}>",
                LocalizableStrings.OutputOptionDescription, CommandOptionType.SingleValue);

            _oneFilePerNamespaceOption = app.Option($"--multiple-files <{LocalizableStrings.MultipleFilesOptionName}>",
                LocalizableStrings.MultipleFilesOptionDescription, CommandOptionType.NoValue);

            app.OnExecute((Func<int>)Execute);

            Reporter.Output.WriteLine("Executing Generate app");
            return app.Execute(args);
        }

        private static int Execute()
        {
            var typifyOptions = new TypifyOptions();

            if (_assemblyOption.HasValue())
            {
                typifyOptions.AssemblyFile = _assemblyOption.Value();
            }
            else
            {
                throw new Exception("Assembly file required.");
            }

            if (_namingStrategyOption.HasValue())
            {
                typifyOptions.NamingStrategy = MapStringToNamingStrategyOption(_namingStrategyOption.Value());
            }

            if (_oneFilePerNamespaceOption.HasValue())
            {
                typifyOptions.OneFilePerNamespace = true;
            }

            if (_outputOption.HasValue())
            {
                typifyOptions.Destination = _outputOption.Value();
            }

            Typifier.Typify(typifyOptions);

            NamingStrategy MapStringToNamingStrategyOption(string namingStrategyOptionStr)
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

            return 0;
        }
    }
}
