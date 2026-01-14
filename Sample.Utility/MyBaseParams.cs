using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Proxy;
using System.CommandLine;

namespace Sample.Utility {
	public class MyBaseParams {
		[Option("c")]
		public int Count { get; init; } = 10;
	}

	public abstract class MyBaseHandler<T> : BaseHandler<T> where T : class {
		protected readonly CommandProxyService commandProxy;
		protected readonly ILogger logger;

		public MyBaseHandler(ParseResult result, CommandProxyService commandProxy, T parameters, ILogger logger) : base(result, parameters) {
			this.commandProxy = commandProxy;
			this.logger = logger;
		}
	}
}