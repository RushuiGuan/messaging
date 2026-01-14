using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Messaging.Utility {
	[Verb<ResetEvents>("reset-events")]
	public class ResetEventsParams {
		[Option("p")]
		public required string Project { get; init; }

		[Option("l")]
		public string? ProjectLocation { get; init; }
	}
	public class ResetEvents : BaseHandler<ResetEventsParams> {
		private readonly ILogger<ResetEvents> logger;

		public ResetEvents(ParseResult result, ResetEventsParams parameters, ILogger<ResetEvents> logger) : base(result, parameters) {
			this.logger = logger;
		}
		public override Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string folder;
			if (string.IsNullOrEmpty(parameters.ProjectLocation)) {
				folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), parameters.Project);
			} else {
				folder = System.IO.Path.Combine(parameters.ProjectLocation, parameters.Project);
			}
			foreach (var file in Directory.GetFiles(folder, "*.log")) {
				logger.LogInformation("Deleting log file: {file}", file);
				File.Delete(file);
			}
			return Task.FromResult(0);
		}
	}
}