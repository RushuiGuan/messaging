using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Core.Commands;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<RunCommandHandlerExceptionTest>("command-handler-exception-test")]
	public class RunCommandHandlerExceptionTestParams {
		[Option("d")]
		public int? Delay { get; init; }
		[Option("c")]
		public bool Callback { get; init; }
	}
	public class RunCommandHandlerExceptionTest : MyBaseHandler<RunCommandHandlerExceptionTestParams> {
		public RunCommandHandlerExceptionTest(ParseResult result, CommandProxyService commandProxy, RunCommandHandlerExceptionTestParams parameters, ILogger<RunCommandHandlerExceptionTest> logger) : base(result, commandProxy, parameters, logger) {
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var cmd = new CommandHandlerExceptionTestCommand {
				Delay = parameters.Delay ?? 0,
				Callback = parameters.Callback,
			};
			await this.commandProxy.SubmitSystemCommand(cmd);
			return 0;
		}
	}
}