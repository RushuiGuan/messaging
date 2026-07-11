using Sample.CommandHandlers;
using Albatross.Messaging.Commands;
using Albatross.Messaging.PubSub;
using Albatross.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sample.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args, AppContext.BaseDirectory) {
		}

		public override void ConfigureServices(IServiceCollection services, IConfiguration cfg) {
			base.ConfigureServices(services, cfg);
			services.AddCommandBus()
				.RegisterCommands(Sample.Extensions.GetQueueName)
				.AddPublisher();
		}
	}
}