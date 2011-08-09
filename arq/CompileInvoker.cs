using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace VersionOne.arq
{
	class CompileInvoker
	{
		private readonly Arguments _arguments;

		public CompileInvoker(Arguments arguments)
		{
			_arguments = arguments;
		}

		public void Run()
		{
			string sourceAsmFilename = Path.GetFullPath(_arguments.Input);
			if (!File.Exists(sourceAsmFilename))
				throw new FileNotFoundException("Input DLL not found", sourceAsmFilename);
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

		public void Install(Assembly sourceAsm)
		{
			string asmPath = sourceAsm.Location;
			string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(asmPath));
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
			                       		ViewFolder = new FileSystemViewFolder(EnsureDirectoryExists(_arguments.Views, "Views"))
			                       	};
			var batch = new SparkBatchDescriptor(GetOutputDllFullPath());
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

		private string EnsureDirectoryExists(string directoryPath, string directoryName)
		{
			if (!Directory.Exists(directoryPath))
				throw new DirectoryNotFoundException(string.Format("{0} folder not found: {1}", directoryName, directoryPath));
			return directoryPath;
		}

		private string GetOutputDllFullPath()
		{
			return Path.Combine(EnsureDirectoryExists(_arguments.OutputPath, "Output"), _arguments.OutputName);
		}

		private static void DescribeSparkViews(SparkBatchDescriptor batch, Assembly sourceAsm)
		{
			sourceAsm
				.GetTypes()
				.Where(type => type.IsSubclassOf(typeof (Controller))).ToList()
				.ForEach(controllerType => batch.For(controllerType));
		}
	}
}