using Albatross.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Albatross.Messaging.CodeGen {
	public static class MySymbolProvider {
		public static INamedTypeSymbol CommandInterfaceAttribute(this Compilation compilation) {
			return compilation.GetRequiredSymbol("Albatross.Messaging.Core.CommandInterfaceAttribute")!;
		}

		public static INamedTypeSymbol ICommandHandlerGenericDefinition1(this Compilation compilation) {
			return compilation.GetRequiredSymbol("Albatross.Messaging.Commands.ICommandHandler`1")!;
		}

		public static INamedTypeSymbol ICommandHandlerGenericDefinition2(this Compilation compilation) {
			return compilation.GetRequiredSymbol("Albatross.Messaging.Commands.ICommandHandler`2")!;
		}
	}
}
