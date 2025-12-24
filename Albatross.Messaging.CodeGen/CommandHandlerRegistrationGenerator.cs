using Albatross.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Albatross.Messaging.CodeGen {
	[Generator]
	public class CommandHandlerRegistrationGenerator : IIncrementalGenerator {
		public void Initialize(IncrementalGeneratorInitializationContext context) {
			var compilationProvider = context.CompilationProvider.Select(static (x, _) => x);
			var handlers = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: static (ctx, _) => {
					var model = ctx.SemanticModel;
					var symbol = model.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node);
					if (symbol != null) {
						foreach(var interfaceType in symbol.AllInterfaces) {
							if(interfaceType.OriginalDefinition.Is(model.Compilation.ICommandHandlerGenericDefinition1()) ||
							   interfaceType.OriginalDefinition.Is(model.Compilation.ICommandHandlerGenericDefinition2())) {
							}
						}
					}
				}
			).Where(static x => x != null);
		}
	}
}
