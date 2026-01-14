using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<RunCommandSerializationErrorTestHandler>("command-serialization-error-test")]
	public class RunCommandSerializationErrorTestParams {
		[Option("c")]
		public bool Callback { get; init; }
	}
	public class RunCommandSerializationErrorTestHandler : BaseHandler<RunCommandSerializationErrorTestParams> {
		private readonly CommandProxyService client;

		public RunCommandSerializationErrorTestHandler(ParseResult result, CommandProxyService client, RunCommandSerializationErrorTestParams parameters) : base(result, parameters) {
			this.client = client;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await client.CommandSerializationErrorTest(parameters.Callback);
			return 0;
		}
	}
}