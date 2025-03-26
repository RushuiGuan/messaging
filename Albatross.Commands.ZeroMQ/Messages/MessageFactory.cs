using NetMQ;

namespace Albatross.Commands.ZeroMQ.Messages {
	public class MessageFactory : IMessageFactory {
		Dictionary<string, IMessageBuilder> builders = new Dictionary<string, IMessageBuilder>();
		void Add(IMessageBuilder builder) {
			this.builders.Add(builder.Header, builder);
		}

		public MessageFactory() {
			Add(new MessageBuilder<CommandErrorReply>());
			Add(new MessageBuilder<CommandReply>());
			Add(new MessageBuilder<CommandRequest>());
			Add(new MessageBuilder<CommandRequestAck>());
			Add(new MessageBuilder<CommandRequestError>());
			Add(new MessageBuilder<ClientAck>());
			Add(new MessageBuilder<Connect>());
			Add(new MessageBuilder<ConnectOk>());
			Add(new MessageBuilder<Heartbeat>());
			Add(new MessageBuilder<HeartbeatAck>());
			Add(new MessageBuilder<Reconnect>());
			Add(new MessageBuilder<Resume>());
			Add(new MessageBuilder<ServerAck>());
			Add(new MessageBuilder<UnknownMsg>());
		}
		public IMessage Create(NetMQMessage frames) {
			var header = frames.PeekMessageHeader();
			IMessage msg;
			if (this.builders.TryGetValue(header, out var builder)) {
				msg = builder.Build(frames);
			} else {
				msg = new UnknownMsg();
				msg.ReadFromFrames(frames);
			}
			return msg;
		}

		public IMessage Create(string line, int offset) {
			var header = line.PeekMessageHeader(offset);
			if (this.builders.TryGetValue(header, out var builder)) {
				return builder.Build(line, offset);
			} else {
				var msg = new UnknownMsg();
				msg.ReadFromText(line, ref offset);
				return msg;
			}
		}
	}

	public interface IMessageFactory {
		IMessage Create(NetMQMessage frames);
		IMessage Create(string line, int offset);
	}
}