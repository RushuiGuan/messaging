namespace Albatross.Commands {
	public interface IRegisterCommand {
		bool HasReturnType { get; }
		IEnumerable<string> Names { get; }
		Type CommandType { get; }
		Type ResponseType { get; }
		string GetQueueName(ulong messageId, object command, IServiceProvider provider);
		ICommandHandler CreateCommandHandler(IServiceProvider provider);
	}
	public interface IRegisterCommand<T> : IRegisterCommand where T : notnull { }
	public interface IRegisterCommand<T, K> : IRegisterCommand where T : notnull where K : notnull { }
}
