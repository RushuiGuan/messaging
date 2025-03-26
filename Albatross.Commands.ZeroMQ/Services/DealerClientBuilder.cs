using Albatross.Commands.ZeroMQ.Configurations;
using Albatross.Commands.ZeroMQ.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Albatross.Commands.ZeroMQ.Services {
	public interface IDealerClientBuilder {
		IDealerClientBuilder TryAddCommandClientService();
		void Build();
	}
	public class DealerClientBuilder : IDealerClientBuilder, IDisposable {
		bool disposed = false;
		IServiceProvider serviceProvider;
		DealerClientConfiguration configuration;
		DealerClient? dealerClient;
		List<IDealerClientService> services = new List<IDealerClientService>();

		public DealerClient DealerClient => this.dealerClient ?? throw new InvalidOperationException("DealerClient instance has not been built");
		
		public CommandClientService CommandClientService
			=> this.services.Where(args => args is CommandClientService).Cast<CommandClientService>()
				.FirstOrDefault() ?? throw new InvalidOperationException("CommandClientService has not been added");


		public DealerClientBuilder(IServiceProvider serviceProvider, DealerClientConfiguration configuration) {
			this.serviceProvider = serviceProvider;
			this.configuration = configuration;
		}

		public IDealerClientBuilder TryAddCommandClientService() {
			if (!services.Any(args => args is CommandClientService)) {
				var registrations = this.serviceProvider.GetRequiredService<IEnumerable<IRegisterCommand>>();
				var logger = this.serviceProvider.GetRequiredService<ILogger<CommandClientService>>();
				services.Add(new CommandClientService(logger));
			}
			return this;
		}

		public void Build() {
			var messageFactory = this.serviceProvider.GetRequiredService<IMessageFactory>();
			var logFactory = this.serviceProvider.GetRequiredService<ILoggerFactory>();
			this.dealerClient = new DealerClient(this.configuration, this.services, messageFactory, logFactory);
		}

		public void Dispose() {
			if (!disposed) {
				this.dealerClient?.Dispose();
				this.disposed = true;
				this.dealerClient = null;
			}
		}
	}
}