namespace Typify.NET.Utilities
{
	using System.Text.RegularExpressions;

	internal static class StringUtils
	{
		public static string ToCamelCase(this string str)
		{
			if (str == null || str.Length < 2)
				return str;

			var words = str.SplitOnCapitalLetters();
			var result = words[0].ToLower();
			for (var i = 1; i < words.Length; i++)
			{
				result +=
					words[i].Substring(0, 1).ToUpper() +
					words[i].Substring(1);
			}

			return result;
		}

		public static string ToSnakeCase(this string str)
		{
			if (str == null || str.Length < 2)
				return str;

			var words = str.SplitOnCapitalLetters();
			for (var i = 0; i < words.Length; i++)
			{
				words[i] = words[i].ToLowerInvariant();
			}
			var result = string.Join("_", words);

			return result;
		}

		private static string[] SplitOnCapitalLetters(this string s)
		{
			var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

			return r.Split(s);
		}
	}
}
