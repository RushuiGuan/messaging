using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<Ping>("ping")]
	public class PingParams { }

	public class Ping : MyBaseHandler<PingParams> {
		public Ping(ParseResult result, CommandProxyService commandProxy, PingParams parameters, ILogger<Ping> logger) : base(result, commandProxy, parameters, logger) {
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await commandProxy.SubmitAppCommand(new Core.Commands.PingCommand(1));
			return 0;
		}
	}
}