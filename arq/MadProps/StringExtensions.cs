namespace MadProps.AppArgs
{
	static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string s)
		{
			return s == null || s.Trim().Length == 0;
		}
	}
}