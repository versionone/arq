using Spark;
using Spark.Web.Mvc;

namespace VersionOne.arq
{
	internal class AutoMasterDescriptorBuilder : DefaultDescriptorBuilder
	{
		public AutoMasterDescriptorBuilder(ISparkViewEngine engine) : base(engine){}

		public override SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, System.Collections.Generic.ICollection<string> searchedLocations)
		{
			var newParams = new BuildDescriptorParams(buildDescriptorParams.TargetNamespace, buildDescriptorParams.ControllerName,
			                                          buildDescriptorParams.ViewName, buildDescriptorParams.MasterName, true,
			                                          buildDescriptorParams.Extra);

			return base.BuildDescriptor(newParams, searchedLocations);
		}
		
	}
}