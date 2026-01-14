using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.Messaging.EventSource;
using Albatross.Messaging.Messages;
using Albatross.Text.CliFormat;
using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Messaging.Utility {
	[Verb<Seek>("seek", Description = "Search the event source files for a conversation with the specified id")]
	public record class SeekParams : MessagingGlobalParams {
		[Option("i", "id")]
		public ulong? Id { get; init; }

		[Option("s", Description = "The local time to start searching for the events")]
		public DateTime? Start { get; init; }

		[Option("e", Description = "The local time to stop searching for the events")]
		public DateTime? End { get; init; }

		[Option("p", "pattern", Description = "Regular expression pattern that can be used as a filter on the message class name")]
		public string? MessageClassNameFilterPattern { get; init; }
	}
	public class Seek : BaseHandler<SeekParams> {
		private readonly IMessageFactory messageFactory;

		public Seek(ParseResult result, IMessageFactory messageFactory, SeekParams parameters) : base(result, parameters) {
			this.messageFactory = messageFactory;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			Conversation? conversation = null;
			foreach (var file in parameters.GetEventSourceFiles()) {
				Writer.WriteLine($"Searching file {file}");
				conversation = await SearchFile(conversation, file, messageFactory, cancellationToken);
			}
			if (conversation != null) {
				Console.Out.CliPrint(conversation, null);
			}
			return 0;
		}

		async Task<Conversation?> SearchFile(Conversation? message, FileInfo file, IMessageFactory messageFactory, CancellationToken cancellationToken) {
			using var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using (var reader = new StreamReader(stream)) {
				while (!reader.EndOfStream) {
					var line = await reader.ReadLineAsync(cancellationToken);
					if (line != null) {
						if (EventEntry.TryParseLine(messageFactory, line, out var entry)) {
							if (parameters.Id == entry.Message.Id) {
								if (message == null) {
									message = new Conversation(entry.Message.Route ?? string.Empty, entry.Message.Id);
								}
								message.Add(entry);
							}
						}
					}
				}
			}
			return message;
		}
	}
}