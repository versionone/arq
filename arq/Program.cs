using System;
using MadProps.AppArgs;

namespace VersionOne.arq
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Arguments arguments;
			if (TryGetArguments(args, out arguments))
				new CompileInvoker(arguments).Run();
		}

		private static bool TryGetArguments(string[] args, out Arguments arguments)
		{
			try
			{
				arguments = args.As<Arguments>();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine();
				var usage = AppArgs.HelpFor<Arguments>();
				Console.Write(usage);

				arguments = null;
				return false;
			}
		}
	}
}