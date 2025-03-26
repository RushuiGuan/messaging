using Albatross.Serialization.Json;
using System;
using System.Text.Json;

namespace Albatross.Commands.ZeroMQ {
	public class MessagingJsonSettings : IJsonSettings {
		public JsonSerializerOptions Value { get; }

		public MessagingJsonSettings() {
			Value = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
			};
		}
		static readonly Lazy<MessagingJsonSettings> lazy = new Lazy<MessagingJsonSettings>();
		public static IJsonSettings Instance => lazy.Value;

	}
}