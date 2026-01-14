using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Albatross.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Threading.Tasks;

namespace Albatross.Messaging.Utility {
	public class Program {
		static async Task<int> Main(string[] args) {
			await using var host = new CommandHost("Albatross Messaging Utility")
				.RegisterServices((_, services) => {
					services.RegisterCommands();
					services.AddSingleton<IMessageFactory, MessageFactory>();
				})
				.AddCommands()
				.Parse(args)
				.WithDefaults()
				.Build();
			return await host.InvokeAsync();
		}
	}
}