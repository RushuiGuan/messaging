using Albatross.Serialization.Json;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.Core.Commands;
using Sample.Proxy;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Utility {
	[Verb<EfficiencyTest>("efficiency-test")]
	public record class EfficiencyTestParams {
		[Option("f")]
		public required FileInfo InputFile { get; init; }
	}
	public class EfficiencyTest : MyBaseHandler<EfficiencyTestParams> {
		public EfficiencyTest(ParseResult result, CommandProxyService commandProxy, EfficiencyTestParams parameters, ILogger<EfficiencyTest> logger)
			: base(result, commandProxy, parameters, logger) {
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			using var stream = this.parameters.InputFile.OpenRead();
			var commands = await JsonSerializer.DeserializeAsync<EfficiencyTestComand[]>(stream, ReducedFootprintJsonSettings.Instance.Value, cancellationToken);
			var ids = new List<ulong>();
			if (commands != null) {
				foreach (var item in commands) {
					var id = await this.commandProxy.SubmitSystemCommand(item);
					ids.Add(id);
				}
			}
			Writer.WriteLine(string.Join(',', ids));
			return 0;
		}
	}
}