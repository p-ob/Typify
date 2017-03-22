namespace Typify.NET.Tools.Help
{
    using Microsoft.Extensions.CommandLineUtils;
    using Typify.NET.Tools.Utils;

    internal class HelpCommand
    {
        public static int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "typify help",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription
            };

            app.OnExecute(() =>
            {
                Reporter.Output.WriteLine("TODO");
                return 0;
            });

            return app.Execute(args);
        }
    }
}
