namespace Typify
{
    public class TypifyOptions
    {
        public NamingStrategy NamingStrategy { get; set; }

        public bool OneFilePerNamespace { get; set; }

        public string Destination { get; set; }

        public TypifyOptions()
        {
            NamingStrategy = NamingStrategy.CamelCase;
            OneFilePerNamespace = false;
            Destination = string.Empty;
        }
    }
}
