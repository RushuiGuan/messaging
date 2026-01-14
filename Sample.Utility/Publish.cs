using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<Publish>("pub")]
	public class PublishParams {
		[Argument]
		public int Min { get; init; }
		[Argument]
		public int Max { get; init; }
		[Option("t")]
		public string Topic { get; init; } = string.Empty;
	}
	public class Publish : BaseHandler<PublishParams> {
		private readonly CommandProxyService client;

		public Publish(ParseResult result, PublishParams parameters, CommandProxyService client) : base(result, parameters) {
			this.client = client;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await client.SubmitAppCommand(new Core.Commands.PublishCommand(parameters.Topic, parameters.Min, parameters.Max));
			return 0;
		}
	}
}