namespace Albatross.Commands {
	/// <summary>
	/// With the exception of the Start and Dispose method, which is used for initialization, 
	/// all other methods in this call should be thread safe.
	/// </summary>
	public interface ICommandClient {
		Task<ulong> Submit(object command, bool fireAndForget = true, int timeout = 2000);
	}
}