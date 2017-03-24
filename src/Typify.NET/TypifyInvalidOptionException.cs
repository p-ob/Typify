namespace Typify.NET
{
    public class TypifyInvalidOptionException : TypifyException
    {
        public readonly string OptionName;

        public readonly object OptionValue;

        public TypifyInvalidOptionException(string optionName, object optionValue, string message) : base(message)
        {
            OptionName = optionName;
            OptionValue = optionValue;
        }
    }
}
