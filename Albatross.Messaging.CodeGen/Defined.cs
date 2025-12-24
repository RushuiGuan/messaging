using Albatross.CodeGen.CSharp.Expressions;

namespace Albatross.Messaging.CodeGen {
	public static class MyDefined {
		public static class Namespaces {
			public readonly static NamespaceExpression SystemTextJsonSerialization = new NamespaceExpression("System.Text.Json.Serialization");
		}
		public static class Identifiers {
			public readonly static QualifiedIdentifierNameExpression JsonDerivedType = new QualifiedIdentifierNameExpression("JsonDerivedType", Namespaces.SystemTextJsonSerialization);
			public readonly static IdentifierNameExpression TypeOf = new IdentifierNameExpression("typeof");
		}
	}
}
