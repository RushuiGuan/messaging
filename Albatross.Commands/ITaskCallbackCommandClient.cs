namespace Albatross.Commands {
	public interface ITaskCallbackCommandClient {
		Task<ulong> SubmitWithCallback(object command, int timeout = 2000);
		Task<T?> SubmitWithCallback<T>(object command, int timeout = 2000);
	}
}