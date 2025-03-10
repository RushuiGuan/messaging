﻿using Albatross.Collections;
using Albatross.Messaging.Configurations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Albatross.Messaging.PubSub.Pub {
	public interface ISubscriptionManagement {
		IEnumerable<Subscription> Subscriptions { get; }
		void Add(string pattern, string route);
		void Remove(string pattern, string route);
		void UnsubscribeAll(string route);
	}
	public class SubscriptionManagement : ISubscriptionManagement {
		private readonly ILogger<SubscriptionManagement> logger;
		private readonly RouterServerConfiguration config;
		public string SubscriptionFilename => Path.Join(config.DiskStorage.WorkingDirectory, "subscriptions.json");
		public SubscriptionManagement(ILogger<SubscriptionManagement> logger, RouterServerConfiguration config) {
			this.logger = logger;
			this.config = config;
			Load();
		}

		ISet<Subscription> subscriptions = new HashSet<Subscription>();
		public IEnumerable<Subscription> Subscriptions => this.subscriptions;

		public void Add(string pattern, string route) {
			var subscription = subscriptions.GetOrAdd(item => item.Pattern == pattern, () => new Subscription(pattern));
			subscription.Subscribers.Add(route);
			Save();
		}

		public void Remove(string pattern, string route) {
			var item = subscriptions.Where(args => args.Pattern == pattern).FirstOrDefault();
			if (item != null) {
				item.Subscribers.Remove(route);
				Save();
			}
		}

		public void UnsubscribeAll(string route) {
			foreach (var sub in subscriptions.ToArray()) {
				sub.Subscribers.Remove(route);
				if (!sub.Subscribers.Any()) {
					subscriptions.Remove(sub);
				}
			}
			Save();
		}

		public void Save() {
			Albatross.IO.Extensions.EnsureDirectory(SubscriptionFilename);
			using (var stream = File.OpenWrite(SubscriptionFilename)) {
				JsonSerializer.Serialize<IEnumerable<Subscription>>(stream, subscriptions);
				stream.Flush();
				stream.SetLength(stream.Position);
			}
		}
		public void Load() {
			var filename = SubscriptionFilename;
			if (File.Exists(filename)) {
				using (var stream = File.OpenRead(filename)) {
					try {
						var items = JsonSerializer.Deserialize<IEnumerable<Subscription>>(stream) ?? new List<Subscription>();
						logger.LogInformation("Loading subscribers");
						foreach (var item in items) {
							logger.LogInformation("Pattern: {pattern}; Subscriber: {subscriber}", item.Pattern, string.Join(',', item.Subscribers));
						}
						this.subscriptions = new HashSet<Subscription>(items);
					} catch (Exception err) {
						logger.LogError(err, "Error reading subscription management info from {file}", filename);
					}
				}
			} else {
				this.subscriptions.Clear();
			}
		}
	}
}