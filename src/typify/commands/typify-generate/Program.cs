namespace Typify.NET.Tools.Generate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.CommandLineUtils;

    internal static class GenerateCommand
    {
        private static CommandOption _assemblyOption;
        private static CommandOption _namingStrategyOption;
        private static CommandOption _outputOption;
        private static CommandOption _oneFilePerNamespaceOption;
        private static CommandArgument _projectArgument;

        private static readonly IEnumerable<string> SupportedProjectTypes = new[] { ".csproj" };

        public static int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "typify generate",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription
            };
            app.HelpOption("-h|--help");

            _projectArgument = app.Argument($"<{LocalizableStrings.ProjectArgumentName}>",
                LocalizableStrings.ProjectArgumentDescription);

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

            var fileOrDir = !string.IsNullOrEmpty(_projectArgument.Value)
                    ? _projectArgument.Value
                    : Directory.GetCurrentDirectory();

            if (_assemblyOption.HasValue())
            {
                typifyOptions.AssemblyFile = _assemblyOption.Value();
            }
            else
            {
                var projectFile = GetProjectFilePath(fileOrDir);
                typifyOptions.AssemblyFile = MsBuildHelper.GetAssemblyForProject(projectFile);
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

        private static string GetProjectFilePath(string fileOrDir)
        {
            var filePath = string.Empty;
            var extension = Path.GetExtension(fileOrDir);
            try
            {
                if (!string.IsNullOrEmpty(extension) && SupportedProjectTypes.Contains(extension))
                {
                    filePath = fileOrDir;
                }
                else
                {
                    var dir = new DirectoryInfo(fileOrDir);
                    var files = dir.GetFiles("*.csproj");

                    var file = files.Single();
                    filePath = file.FullName;
                }
            }
            catch (Exception)
            {
                throw new TypifyException("Could not find project to load.");
            }

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                return filePath;
            }
            throw new TypifyException("Could not find project to load.");
        }
    }
}
