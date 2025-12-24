using Albatross.CodeGen.CSharp.Expressions;

namespace Albatross.Messaging.CodeGen {
	public static class MyDefined {
		public static class Namespaces {
			public static readonly NamespaceExpression SystemTextJsonSerialization = new NamespaceExpression("System.Text.Json.Serialization");
			public static readonly NamespaceExpression AlbatrossMessagingCommands = new NamespaceExpression("Albatross.Messaging.Commands");
		}

		public static class Identifiers {
			public static readonly QualifiedIdentifierNameExpression JsonDerivedType = new("JsonDerivedType", Namespaces.SystemTextJsonSerialization);
			public static readonly IdentifierNameExpression TypeOf = new IdentifierNameExpression("typeof");
			public static readonly QualifiedIdentifierNameExpression ICommandHandler = new("ICommandHandler", Namespaces.AlbatrossMessagingCommands);
		}
	}
}