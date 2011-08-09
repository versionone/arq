using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace VersionOne.arq
{
	class Arguments
	{
		[Required, Description("the assembly containing MVC controllers")]
		public string Source { get; set; }

		private string _output;

		[Description("name of output assembly (defaults to <input>.Views.dll")]
		public string Output
		{
			get { return _output ?? Path.GetFileNameWithoutExtension(Source) + ".Views.dll"; }
			set { _output = value; }
		}

		internal string SourcePath
		{
			get { return Path.GetDirectoryName(Path.GetFullPath(Source)); }
		}

		internal string SourceName
		{
			get { return Path.GetFileName(Source); }
		}

		internal string OutputPath
		{
			get
			{
				if (_output == null) return SourcePath;
				var path = _output;
				if (!Path.IsPathRooted(path))
					path = Path.Combine(SourcePath, path);
				if (path.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
					path = Path.GetDirectoryName(path);
				return Path.GetFullPath(path);
			}
		}

		internal string OutputName
		{
			get
			{
				if (_output == null || !_output.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)) 
					return Path.ChangeExtension(SourceName, ".Views.dll");
				return Path.GetFileName(_output);
			}
		}
	}
}