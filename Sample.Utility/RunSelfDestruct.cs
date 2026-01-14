using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Core.Commands;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<RunSelfDestruct>("self-destruct")]
	public class RunSelfDestructParams {
		[Option("d")]
		public int? Delay { get; init; }
	}
	public class RunSelfDestruct : MyBaseHandler<RunSelfDestructParams> {
		public RunSelfDestruct(ParseResult result, CommandProxyService commandProxy, RunSelfDestructParams parameters, ILogger<RunSelfDestruct> logger) : base(result, commandProxy, parameters, logger) {
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var cmd = new SelfDestructCommand() {
				Delay = parameters.Delay,
			};
			await commandProxy.SubmitSystemCommand(cmd);
			return 0;
		}
	}
}