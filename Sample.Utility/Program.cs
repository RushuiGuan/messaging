using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Sample.Proxy;
using System.Threading.Tasks;

namespace Sample.Utility {
	public class Program {
		static async Task<int> Main(string[] args) {
			await using var host = new CommandHost("Sample Utility")
				.RegisterServices((_, services) => {
					services.RegisterCommands();
					services.AddSampleProjectProxy();
				})
				.AddCommands()
				.Parse(args)
				.WithDefaults()
				.Build();
			return await host.InvokeAsync();
		}
	}
}