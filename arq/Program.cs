using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;
using MadProps.AppArgs;

namespace VersionOne.arq
{
	class Arguments
	{
		[Required, Description("the assembly containing MVC controllers")]
		public string Source { get; set; }
	}

	class CompileInvoker
	{
		private readonly Arguments _arguments;

		public CompileInvoker(Arguments arguments)
		{
			_arguments = arguments;
		}
		public void Run() 
		{

			string sourceAsmFilename = Path.GetFullPath(_arguments.Source);
			if (!File.Exists(sourceAsmFilename))
				throw new FileNotFoundException("No such file", sourceAsmFilename);
			Install(GetAssembly(sourceAsmFilename));
		}

		private static Assembly GetAssembly(string filename)
		{
			string asmPath = Path.GetDirectoryName(filename);
			AppDomain.CurrentDomain.AssemblyResolve += (ResolveEventHandler) ((sender, e) =>
			                                                                  	{
			                                                                  		var assemblyName = new AssemblyName(e.Name);
			                                                                  		return
			                                                                  			Assembly.LoadFile(Path.Combine(asmPath,
			                                                                  			                               assemblyName.Name +
			                                                                  			                               ".dll"));
			                                                                  	});
			return Assembly.LoadFile(filename);
		}

		public static void Install(Assembly sourceAsm)
		{
			string asmPath = sourceAsm.Location;
			string targetAsmFilename = Path.ChangeExtension(asmPath, ".Views.dll");
			string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(asmPath));
			string viewsPath = Path.Combine(projectPath, "IceNine\\Views");
			targetAsmFilename = Path.Combine(asmPath, targetAsmFilename);
			ISparkSettings settings;
			{
				string str = Path.Combine(projectPath, "web");
				File.Create(str).Close();
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(str);
				File.Delete(str);
				settings = (ISparkSettings) configuration.GetSection("spark");
			}
			var sparkViewFactory = new SparkViewFactory(settings)
			                       	{
			                       		ViewFolder = new FileSystemViewFolder(viewsPath)
			                       	};
			var batch = new SparkBatchDescriptor(targetAsmFilename);
			batch.FromAssembly(sourceAsm);
			DescribeSparkViews(batch, sourceAsm);

			try
			{
				sparkViewFactory.Precompile(batch);
			}
			catch (Exception e)
			{
				File.WriteAllText("SparkCompile.log.txt", e.Message);
				throw new Exception("Compilation failed.  See log file for details", e);
			}
		}

		private static void DescribeSparkViews(SparkBatchDescriptor batch, Assembly sourceAsm)
		{
			sourceAsm
				.GetTypes()
				.Where(type => type.IsSubclassOf(typeof (Controller))).ToList()
				.ForEach(controllerType => batch.For(controllerType));
		}
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			Arguments arguments;

			if (!TryGetArguments(args, out arguments))
				return;
				
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
				arguments = null;

				Console.WriteLine(ex.Message);
				Console.WriteLine();
				var usage = AppArgs.HelpFor<Arguments>();
				Console.Write(usage);

				return false;
			}
		}
	}
}