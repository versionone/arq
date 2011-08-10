	static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}

		private static readonly char[] EolChars = new char[] { '\n', '\r' };

		public static string FirstLine(this string text)
		{
			var i = text.IndexOfAny(EolChars);
			return i < 0 ? text : text.Substring(0, i);
		}
	}
