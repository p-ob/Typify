﻿namespace Typify.NET
{
    public class TypifyOptions
    {
        public NamingStrategy NamingStrategy { get; set; }

        public bool OneFilePerNamespace { get; set; }

        public string Destination { get; set; }

        public string AssemblyFile { get; set; }

        public string TargetTypeScriptVersion { get; set; }

        public TypifyOptions()
        {
            NamingStrategy = NamingStrategy.CamelCase;
            OneFilePerNamespace = false;
            Destination = string.Empty;
            AssemblyFile = string.Empty;
            TargetTypeScriptVersion = "2.2";
        }
    }
}
