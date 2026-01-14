using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.Proxy;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {

	[Verb<UnsubscribeAll>("unsub-all")]
	public class UnsubscribeAllParams { }
	public class UnsubscribeAll : BaseHandler<UnsubscribeAllParams> {
		private readonly RunProxyService svc;

		public UnsubscribeAll(ParseResult result, RunProxyService svc, UnsubscribeAllParams parameters) : base(result, parameters) {
			this.svc = svc;
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await svc.UnsubscribeAll();
			return 0;
		}
	}
}