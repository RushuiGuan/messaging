using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Core.Commands;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<TestCustomDealerClientSetup>("test-custom-dealer-client")]
	public class TestCustomDealerClientSetupParams : MyBaseParams {
	}
	public class TestCustomDealerClientSetup : MyBaseHandler<TestCustomDealerClientSetupParams> {
		public TestCustomDealerClientSetup(ParseResult result, CommandProxyService commandProxy, TestCustomDealerClientSetupParams parameters, ILogger<TestCustomDealerClientSetup> logger) : base(result, commandProxy, parameters, logger) {
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await commandProxy.SubmitSystemCommand(new MyCommand1("test command 1"));
			logger.LogInformation("Existing command");
			return 0;
		}
	}
}