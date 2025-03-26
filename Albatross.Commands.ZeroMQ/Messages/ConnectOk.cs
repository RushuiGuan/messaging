using Albatross.Commands.ZeroMQ.Messages;
using NetMQ;

namespace Albatross.Commands.ZeroMQ.Messages {
	/// <summary>
	/// a message reply from the server to the client when the server receives a <see cref="Albatross.Commands.ZeroMQ.Messages.Connect"/> message.
	/// </summary>
	public record class ConnectOk : Message, IMessage, ISystemMessage {
		public static string MessageHeader => "connect-ok";
		public ConnectOk(string route, ulong id) : base(MessageHeader, route, id) { }
		public ConnectOk() { }
	}
}