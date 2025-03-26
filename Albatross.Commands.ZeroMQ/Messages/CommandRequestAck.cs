using Albatross.Commands.ZeroMQ.Messages;
using NetMQ;

namespace Albatross.Commands.ZeroMQ.Messages {
	public record class CommandRequestAck : Message, IMessage {
		public static string MessageHeader => "cmd-req-ack";
		public CommandRequestAck(string route, ulong id) : base(MessageHeader, route, id) { }
		public CommandRequestAck() { }
	}
}