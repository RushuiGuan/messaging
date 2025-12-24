using Albatross.CodeAnalysis;
using Albatross.CodeGen;
using Albatross.CodeGen.CSharp;
using Albatross.CodeGen.CSharp.Declarations;
using Albatross.CodeGen.CSharp.Expressions;
using Albatross.CodeGen.CSharp.TypeConversions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Albatross.Messaging.CodeGen {
	[Generator]
	public class CommandLineCodeGenerator : IIncrementalGenerator {
		public void Initialize(IncrementalGeneratorInitializationContext context) {
			var compilationProvider = context.CompilationProvider.Select(static (x, _) => x);

			// looking for partial interfaces that end with "Command"
			var interfaces = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: static (node, _) => node is InterfaceDeclarationSyntax syntax,
				transform: static (ctx, _) => {
					var interfaceDeclaration = (InterfaceDeclarationSyntax)ctx.Node;
					var model = ctx.SemanticModel;
					var interfaceType =	model.GetDeclaredSymbol(interfaceDeclaration);
					if (interfaceType != null && interfaceType.IsPartial()) {
						if (interfaceType.Name.EndsWith("Command", StringComparison.Ordinal) && interfaceType.GetMembers().Length == 0) {
							return interfaceType;
						} else if (interfaceType.HasAttribute(model.Compilation.CommandInterfaceAttribute())) {
							return interfaceType;
						}
					}
					return null;
				}
			).Where(x => x != null)
			.WithComparer(SymbolEqualityComparer.Default);

			var classes = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
				transform: static (ctx, _) => {
					var model = ctx.SemanticModel;
					return model.GetDeclaredSymbol((BaseTypeDeclarationSyntax)ctx.Node);
				}
			).Where(static x => x != null)
			.WithComparer(SymbolEqualityComparer.Default)
			.Combine(interfaces.Collect()).Select(static (tuple, _) => {
				var (classType, interfaceTypeSet) = tuple;
				var list = new List<(INamedTypeSymbol classType, INamedTypeSymbol interfaceType)>();
				foreach (var interfaceType in interfaceTypeSet) {
					if (classType.IsConcreteClass() && classType!.AllInterfaces.Contains(interfaceType!, SymbolEqualityComparer.Default)) {
						list.Add((classType!, interfaceType!));
					}
				}
				return list;
			}).SelectMany(static (x, _) => x);

			var aggregated = compilationProvider.Combine(classes.Collect());

			context.RegisterSourceOutput(aggregated, static (context, tuple) => {
				var (compilation, pair) = tuple;
				var typeConverter = new DefaultTypeConverter(compilation);
				foreach (var group in pair.GroupBy(x => x.interfaceType, SymbolEqualityComparer.Default)) {
					var file = new FileDeclaration(group.Key!.Name + ".g") {
						Namespace = new NamespaceExpression(group.Key.ContainingNamespace.GetFullNamespace()),
						Interfaces = [
							new InterfaceDeclaration(group.Key.Name) {
								IsPartial = true,
								AccessModifier = Defined.Keywords.Public,
								Attributes = group.Select(x => new AttributeExpression {
									CallableExpression = MyDefined.Identifiers.JsonDerivedType,
									Arguments = new (
										new InvocationExpression {
											CallableExpression = MyDefined.Identifiers.TypeOf,
											Arguments = new (typeConverter.Convert(x.classType))
										},
										new StringLiteralExpression(x.classType.Name)
									)
								})
							}
						]
					};
					context.AddSource(file.FileName, new StringWriter().Code(file).ToString());
				}
			});
		}
	}
}