using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace VersionOne.arq
{
	class Arguments
	{
		[Required, Description("the assembly containing MVC controllers")]
		public string Input { get; set; }


		[Description("name of output assembly (defaults to <input>.Views.dll")]
		public string Output { private get; set; }

		private string _views;

		[Description("path to the MVC views folder relative to <input> (defaults to ..\\views) ")]
		public string Views
		{
			get { return Path.GetFullPath(Path.Combine(InputPath, _views ?? "..\\views")); }
			set { _views = value; }
		}

		private string _config;

		[Description("path to the config that contains spark settings relative to <input> (defaults to ..\\web.config) ")]
		public string Config
		{
			get { return Path.GetFullPath(Path.Combine(InputPath, _config ?? "..\\web.config")); }
			set { _config = value; }
		}

		internal string InputPath
		{
			get { return Path.GetDirectoryName(Path.GetFullPath(Input)); }
		}

		internal string InputName
		{
			get { return Path.GetFileName(Input); }
		}

		internal string OutputPath
		{
			get
			{
				if (Output == null) return InputPath;
				var path = Output;
				if (!Path.IsPathRooted(path))
					path = Path.Combine(InputPath, path);
				if (path.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
					path = Path.GetDirectoryName(path);
				return Path.GetFullPath(path);
			}
		}

		internal string OutputName
		{
			get
			{
				if (Output == null || !Output.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)) 
					return Path.ChangeExtension(InputName, ".Views.dll");
				return Path.GetFileName(Output);
			}
		}
	}
}