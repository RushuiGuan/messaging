using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Albatross.Commands.ZeroMQ.Messages {
	public record class UnknownMsg : Message, IMessage {
		public static string MessageHeader => "unknown";
		public UnknownMsg() { }
		public List<byte[]> PayLoad { get; private set; } = new List<byte[]>();
	}
}