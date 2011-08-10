using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Spark;
using Spark.Compiler;
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
			Compile(GetAssembly(EnsureFileExists(_arguments.InputFilePath)));
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

		public void Compile(Assembly sourceAsm)
		{
			var sparkViewFactory = new SparkViewFactory(GetSparkSettings())
			                       	{
			                       		ViewFolder = new FileSystemViewFolder(EnsureDirectoryExists(_arguments.Views))
			                       	};
			var batch = new SparkBatchDescriptor(GetOutputDllFullPath());
			batch.FromAssembly(sourceAsm);
			
			DescribeSparkViews(batch, sourceAsm);
			DescribeCustomSparkViews(batch, sourceAsm);

			sparkViewFactory.DescriptorBuilder = new AutoMasterDescriptorBuilder(sparkViewFactory.Engine);

			try
			{
				sparkViewFactory.Precompile(batch);
			}
			catch (CompilerException e)
			{
				File.WriteAllText("arq.CompilerException.txt", e.Message);
				throw new CompilerException(e.Message.FirstLine() + "\nSee arq.CompilerException.txt for full description");
			}
		}

		private void DescribeCustomSparkViews(SparkBatchDescriptor batch, Assembly sourceAsm)
		{
			string rules = _arguments.Rules;
			if (rules != null)
			{
				var rulesTypeName = rules.Substring(0, rules.LastIndexOf('.'));
				var rulesMethodName = rules.Substring(rules.LastIndexOf('.') + 1);
				var rulesType = sourceAsm.GetType(rulesTypeName, true);
				rulesType.InvokeMember(rulesMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null,
				                       null, new object[] {batch});
			}
		}

		private ISparkSettings GetSparkSettings()
		{
			var map = new ExeConfigurationFileMap {ExeConfigFilename = EnsureFileExists(_arguments.Config)};
			var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

			return (ISparkSettings) config.GetSection("spark");
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