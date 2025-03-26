namespace Albatross.Commands.ZeroMQ.Configurations {
	public class SubscriptionManagementConfiguration {
		public DiskStorageConfiguration DiskStorage { get; set; } = new DiskStorageConfiguration(null, "subscription.json");
	}
}