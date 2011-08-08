using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VersionOne.arq
{
	class Arguments
	{
		[Required, Description("the assembly containing MVC controllers")]
		public string Source { get; set; }
	}
}