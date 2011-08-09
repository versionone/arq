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
			ISparkSettings settings;
			{
				var map = new ExeConfigurationFileMap();
				map.ExeConfigFilename = EnsureFileExists(_arguments.Config);
				var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
				
				settings = (ISparkSettings) config.GetSection("spark");
			}

			var sparkViewFactory = new SparkViewFactory(settings)
			                       	{
			                       		ViewFolder = new FileSystemViewFolder(EnsureDirectoryExists(_arguments.Views))
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

		private string EnsureFileExists(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException(filePath);
			return filePath;
		}

		private string EnsureDirectoryExists(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
				throw new DirectoryNotFoundException(directoryPath);
			return directoryPath;
		}

		private string GetOutputDllFullPath()
		{
			return Path.Combine(EnsureDirectoryExists(_arguments.OutputPath), _arguments.OutputName);
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