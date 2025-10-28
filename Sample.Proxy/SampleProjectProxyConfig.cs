using Albatross.Config;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Sample.Proxy {
	public class SampleProjectProxyConfig : ConfigBase {
		public SampleProjectProxyConfig(IConfiguration configuration) : base(configuration, null) {
			EndPoint = configuration.GetRequiredEndPoint("sample-project")!;
		}

		public string EndPoint { get; set; }
	}
}