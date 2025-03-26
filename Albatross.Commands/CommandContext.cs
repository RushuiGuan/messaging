namespace Albatross.Commands {
	public record class CommandContext {
		public Guid ContextId { get; }
		public CommandContext() {
			ContextId = Guid.NewGuid();
		}

		public ulong Id { get; set; }
		public string Route { get; set; } = string.Empty;
		public string Queue { get; set; } = string.Empty;
		public CommandMode Mode { get; set; }

		public List<ulong> InternalCommands { get; } = new List<ulong>();
	}
}