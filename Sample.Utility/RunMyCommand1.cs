using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Core.Commands;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<RunMyCommand1>("my-cmd1")]
	public class RunMyCommand1Params : MyBaseParams {
		[Option("d")]
		public int Delay { get; init; }

		[Option("t")]
		public string? Text { get; init; }

		[Option("e")]
		public bool Error { get; init; }

		[Option("child-count")]
		public int ChildCount { get; init; }
	}
	public class RunMyCommand1 : BaseHandler<RunMyCommand1Params> {
		private readonly CommandProxyService client;

		public RunMyCommand1(ParseResult result, CommandProxyService client, RunMyCommand1Params parameters) : base(result, parameters) {
			this.client = client;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			for (int i = 0; i < parameters.Count; i++) {
				var cmd = new MyCommand1($"test command {i}") {
					Error = parameters.Error,
					Delay = parameters.Delay,
				};
				await client.SubmitSystemCommand(cmd);
			}
			return 0;
		}
	}
}