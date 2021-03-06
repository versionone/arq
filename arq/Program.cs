﻿using System;
using MadProps.AppArgs;

namespace VersionOne.arq
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			Arguments arguments;
			if (TryGetArguments(args, out arguments))
				return Run(arguments);
			return 1;
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
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(ex.Message);
				Console.WriteLine();
				Console.ResetColor();
				var usage = AppArgs.HelpFor<Arguments>();
				Console.Write(usage);

				arguments = null;
				return false;
			}
		}

		private static int Run(Arguments arguments)
		{
			try
			{
				new CompileInvoker(arguments).Run();
				return 0;
			}
			catch (Exception ex)
			{
				while (ex != null)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(ex.GetType() + ": " + ex.Message);
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.WriteLine(ex.StackTrace);
					Console.ResetColor();
					ex = ex.InnerException;
				}
				return 2;
			}
		}
	}
}