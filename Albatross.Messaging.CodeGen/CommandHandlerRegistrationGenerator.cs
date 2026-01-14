using Albatross.CodeAnalysis;
using Albatross.CodeGen;
using Albatross.CodeGen.CSharp;
using Albatross.CodeGen.CSharp.Declarations;
using Albatross.CodeGen.CSharp.Expressions;
using Albatross.CodeGen.CSharp.TypeConversions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Albatross.Messaging.CodeGen {
	/// <summary>
	/// <code><![CDATA[
	/// namespace Sample.CommandHandlers {
	///		public static class CodeGenExtensions {
	///			public static IServiceCollection RegisterCommands(this IServiceCollection services, Func<ulong, object, IServiceProvider, string> getQueueName) {
	///				services.AddScoped<ICommandHandler<CommandHandlerExceptionTestCommand>, CommandHandlerExceptionTestCommandHandler>();
	///				services.AddCommand<CommandHandlerExceptionTestCommand>(new string[] { "CommandHandlerExceptionTestCommand" }, getQueueName);
	///				return services;
	///			}
	///		}
	/// }
	/// ]]></code>
	/// </summary>
	[Generator]
	public class CommandHandlerRegistrationGenerator : IIncrementalGenerator {
		class CommandHandlerInfo {
			public INamedTypeSymbol InterfaceType { get; }
			public INamedTypeSymbol ImplementationType { get; }
			public required INamedTypeSymbol CommandType { get; init; }
			public INamedTypeSymbol? ResponseType { get; init; }

			public CommandHandlerInfo(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType) {
				InterfaceType = interfaceType;
				ImplementationType = implementationType;
			}
		}


		private static IExpression CreateRegistrationExpression(Compilation compilation, CommandHandlerInfo info, DefaultTypeConverter typeConverter) {
			return new InvocationExpression {
				CallableExpression = new IdentifierNameExpression("services.AddScoped") {
					GenericArguments = new() {
						typeConverter.Convert(info.InterfaceType),
						typeConverter.Convert(info.ImplementationType),
					}
				},
			};
		}

		private static IExpression CreateAddCommandExpression(Compilation compilation, CommandHandlerInfo info, DefaultTypeConverter typeConverter) {
			var commandType = info.InterfaceType.TypeArguments[0];
			var commandName = commandType.Name;
			return new InvocationExpression {
				CallableExpression = new IdentifierNameExpression("services.AddCommand") {
					GenericArguments = new() {
						typeConverter.Convert(commandType),
						{  info.ResponseType != null, ()=> typeConverter.Convert(info.ResponseType!) }
					},
				},
				Arguments = {
					new ArrayExpression {
						Type = Defined.Types.String,
						Items = { new StringLiteralExpression(commandName) }
					},
					new IdentifierNameExpression("getQueueName"),
				}
			};
		}

		public void Initialize(IncrementalGeneratorInitializationContext context) {
			var compilationProvider = context.CompilationProvider.Select(static (x, _) => x);
			var handlers = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: static (ctx, _) => {
					var model = ctx.SemanticModel;
					var symbol = model.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node);
					if (symbol != null && symbol.IsConcreteClass()) {
						foreach (var interfaceType in symbol.AllInterfaces) {
							if (interfaceType.IsGenericType) {
								if (interfaceType.OriginalDefinition.Is(model.Compilation.ICommandHandlerGenericDefinition1()) ||
									interfaceType.OriginalDefinition.Is(model.Compilation.ICommandHandlerGenericDefinition2())) {
									return new CommandHandlerInfo(interfaceType, symbol) {
										CommandType = (INamedTypeSymbol)interfaceType.TypeArguments[0],
										ResponseType = interfaceType.TypeArguments.Length == 2 ? (INamedTypeSymbol)interfaceType.TypeArguments[1] : null,
									};
								}
							}
						}
					}
					return null;
				}
			).Where(static x => x != null);
			var aggregate = compilationProvider.Combine(handlers.Collect());
			context.RegisterSourceOutput(aggregate, static (context, tuple) => {
				var (compilation, handlerSymbols) = tuple;
				int index = 0;
				var typeConverter = new DefaultTypeConverter {
					UseQualifiedNames = true,
				};
				foreach (var group in handlerSymbols.GroupBy<CommandHandlerInfo, INamespaceSymbol>(x => x.ImplementationType.ContainingNamespace, SymbolEqualityComparer.Default)) {
					var file = new FileDeclaration($"CommandHandlerRegistration{index}.g") {
						Namespace = new NamespaceExpression(group.Key.GetFullNamespace()),
						Classes = [
							new ClassDeclaration {
								IsStatic = true,
								AccessModifier = Defined.Keywords.Public,
								Name = new IdentifierNameExpression("CommandHandlerRegistration"),
								Methods = [
									new MethodDeclaration {
										Name = new IdentifierNameExpression("RegisterCommands"),
										IsStatic = true,
										AccessModifier = Defined.Keywords.Public,
										Parameters = new() {
											new ParameterDeclaration {
												Type = Defined.Types.IServiceCollection,
												Name = new IdentifierNameExpression("services"),
												UseThisKeyword = true,
											},
											new ParameterDeclaration {
												Type = new TypeExpression(new QualifiedIdentifierNameExpression("Func", Defined.Namespaces.System) {
													GenericArguments = [
														new TypeExpression("ulong"),
														Defined.Types.Object,
														new TypeExpression(new QualifiedIdentifierNameExpression("IServiceProvider", Defined.Namespaces.System)),
														Defined.Types.String,
													]
												}),
												Name = new IdentifierNameExpression("getQueueName"),
											}
										},
										Body = {
											{
												true, () => group.Select(x => CreateRegistrationExpression(compilation, x!, typeConverter))
											}, {
												true, () => group.Select(x => CreateAddCommandExpression(compilation, x!, typeConverter))
											},
											new ReturnExpression {
												Expression = new IdentifierNameExpression("services")
											}
										},
										ReturnType = Defined.Types.IServiceCollection,
									}
								]
							}
						]
					};
					var writer = new System.IO.StringWriter();
					file.Generate(writer);
					context.AddSource(file.FileName, writer.ToString());
					index++;
				}
			});
		}
	}
}