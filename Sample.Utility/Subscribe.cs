using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {

	[Verb<Subscribe>("sub")]
	public class SubscribeParams {
		[Option("o")]
		public bool On { get; init; }

		[Option("t")]
		public string Topic { get; init; } = string.Empty;
	}
	public class Subscribe : BaseHandler<SubscribeParams> {
		private readonly RunProxyService svc;

		public Subscribe(ParseResult result, RunProxyService svc, SubscribeParams parameters) : base(result, parameters) {
			this.svc = svc;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			if (parameters.On) {
				await svc.Subscribe(parameters.Topic);
			} else {
				await svc.Unsubscribe(parameters.Topic);
			}
			return 0;
		}
	}
}