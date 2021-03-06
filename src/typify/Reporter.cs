﻿namespace Typify.NET.Tools
{
    using Microsoft.Extensions.CommandLineUtils;

    internal static class Reporter
    {
        public static readonly AnsiConsole Error;
        public static readonly AnsiConsole Output;

        static Reporter()
        {
            Error = AnsiConsole.GetError(true);
            Output = AnsiConsole.GetOutput(true);
        }
    }
}
