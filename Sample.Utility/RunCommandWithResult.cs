using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Core.Commands;
using Sample.Proxy;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<RunCommandWithResult>("command-with-result")]
	public class RunCommandWithResultParams {
		[Option("n")]
		public string Name { get; init; } = string.Empty;

		[Option("v")]
		public int? Value { get; init; }

		[Option("c")]
		public bool Callback { get; init; }
	}
	public class RunCommandWithResult : BaseHandler<RunCommandWithResultParams> {
		private readonly CommandProxyService client;

		public RunCommandWithResult(ParseResult result, CommandProxyService client, RunCommandWithResultParams parameters) : base(result, parameters) {
			this.client = client;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var cmd = new TestOperationWithResultCommand(parameters.Name) {
				Value = parameters.Value ?? new Random().Next(),
				Callback = parameters.Callback,
			};
			var id = await client.SubmitSystemCommand(cmd);
			Writer.WriteLine($"Command submitted with id {id}");
			return 0;
		}
	}
}