namespace Typify.NET.Tools
{
    internal class AppArg
    {
        public readonly string LongName;

        public readonly string ShortName;

        public AppArg(string longName, string shortName)
        {
            LongName = longName;
            ShortName = shortName;
        }

        public bool IsArg(string candidate)
        {
            return ShortName != null && candidate.Equals($"-{ShortName}") ||
                   LongName != null && candidate.Equals($"--{LongName}");
        }
    }
}
