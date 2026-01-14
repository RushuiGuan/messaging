using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.Messaging.EventSource;
using Albatross.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Messaging.Utility {
	[Verb<Stat>("stat")]
	public record class StatParams : MessagingGlobalParams {
		[Option("s", Description = "The local time to start searching for the events")]
		public DateTime? Start { get; init; }
		[Option("e", Description = "The local time to stop searching for the events")]
		public DateTime? End { get; init; }
		[Option("p", "pattern", Description = "Regular expression pattern that can be used as a filter on the message class name")]
		public string? MessageClassNameFilterPattern { get; init; }
	}

	public class Stat : BaseHandler<StatParams> {
		private readonly IMessageFactory messageFactory;

		public Stat(ParseResult result, IMessageFactory messageFactory, StatParams parameters) : base(result, parameters) {
			this.messageFactory = messageFactory;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var start = DateTime.SpecifyKind(parameters.Start ?? DateTime.MinValue, DateTimeKind.Local);
			var end = DateTime.SpecifyKind(parameters.End ?? DateTime.Now, DateTimeKind.Local);
			var files = parameters.GetEventSourceFiles();
			var filesToSearch = files.FindFilesToSearch(start, end);
			var conversations = new Dictionary<ulong, Conversation>();
			Regex? regex = null;
			if (parameters.MessageClassNameFilterPattern != null) {
				regex = new Regex(parameters.MessageClassNameFilterPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			foreach (var file in filesToSearch) {
				using var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using (var reader = new StreamReader(stream)) {
					while (!reader.EndOfStream) {
						var line = await reader.ReadLineAsync(cancellationToken);
						if (line != null) {
							if (EventEntry.TryParseLine(messageFactory, line, out var entry)) {
								if (conversations.TryGetValue(entry.Message.Id, out var messageGroup)) {
									messageGroup.Add(entry);
								} else {
									if (entry.TimeStamp >= start && entry.TimeStamp <= end) {
										messageGroup = new Conversation(entry.Message.Route ?? string.Empty, entry.Message.Id);
										conversations.Add(entry.Message.Id, messageGroup);
									}
								}
							}
						}
					}
				}
			}
			return 0;
		}
	}
}