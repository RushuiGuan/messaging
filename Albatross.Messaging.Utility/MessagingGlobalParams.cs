using Albatross.CommandLine.Annotations;

namespace Albatross.Messaging.Utility {
	public record class MessagingGlobalParams {
		[Option("a")]
		public required string Application { get; init; }

		[Option("f")]
		public string? EventSourceFolder { get; init; }
	}
}
