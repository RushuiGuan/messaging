using Albatross.Commands.ZeroMQ.Messages;
using Albatross.Commands.ZeroMQ.Services;
using Albatross.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetMQ;
using System;
using System.Text;

namespace Albatross.Commands.ZeroMQ {
	public static class Extensions {
		public const string DefaultQueueName = "default_queue";
		public static string GetDefaultQueueName(ulong messageId, object _, IServiceProvider provider) => DefaultQueueName;

		public static IServiceCollection AddCommand<T>(this IServiceCollection services, string[] names, Func<ulong, T, IServiceProvider, string> getQueueName) where T : notnull {
			services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRegisterCommand), new RegisterCommand<T>(names, getQueueName)));
			return services;
		}
		public static IServiceCollection AddCommand<T, K>(this IServiceCollection services, string[] names, Func<ulong, T, IServiceProvider, string> getQueueName) where T : notnull where K : notnull {
			services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRegisterCommand), new RegisterCommand<T, K>(names, getQueueName)));
			return services;
		}

		public static IServiceCollection AddCommandClient<T>(this IServiceCollection services) where T : class, ICommandClient {
			services.TryAddSingleton<CommandClientService>();
			services.AddSingleton<IDealerClientService>(args => args.GetRequiredService<CommandClientService>());
			services.TryAddSingleton<ICommandClient, T>();
			services.AddDealerClient();
			return services;
		}

		public static IServiceCollection AddCommandClient(this IServiceCollection services) => AddCommandClient<CommandClient>(services);

		public static IServiceCollection AddCommandBus(this IServiceCollection services) {
			services.TryAddSingleton<ICommandBusService, CommandBusService>();
			services.AddSingleton<IRouterServerService>(provider => provider.GetRequiredService<ICommandBusService>());
			services.TryAddSingleton<ICommandQueueFactory, CommandQueueFactory>();
			services.TryAddTransient<CommandQueue>();
			services.TryAddScoped<InternalCommandClient>();
			services.TryAddScoped<CommandContext>();
			services.AddRouterServer();
			return services;
		}

		public static Task SubmitCollection(this ICommandClient client, IEnumerable<object> commands, bool fireAndForget = true, int timeout = 2000) {
			var tasks = new List<Task>();
			foreach (var cmd in commands) {
				tasks.Add(client.Submit(cmd, fireAndForget, 0));
			}
			return tasks.WithTimeOut(TimeSpan.FromMilliseconds(timeout));
		}

		public static void ClientAck(this IMessagingService svc, string route, ulong id) => svc.Transmit(new ClientAck(route, id));
		public static void ServerAck(this IMessagingService svc, string route, ulong id) => svc.Transmit(new ServerAck(route, id));

		public static byte[] ToUtf8Bytes(this string text) => Encoding.UTF8.GetBytes(text);

		public static string ToUtf8String(this byte[] data) => Encoding.UTF8.GetString(data);
		public static double ToDouble(this byte[] data) => BitConverter.ToDouble(data);
		public static int ToInt(this byte[] data) => BitConverter.ToInt32(data);
		public static ulong ToULong(this byte[] data) => BitConverter.ToUInt64(data);
		public static long ToLong(this byte[] data) => BitConverter.ToInt64(data);
		public static bool ToBoolean(this byte[] data) => BitConverter.ToBoolean(data);

		public static string PopUtf8String(this NetMQMessage frames) => Encoding.UTF8.GetString(frames.Pop().Buffer);
		public static void AppendUtf8String(this NetMQMessage frames, string? text) {
			if (string.IsNullOrEmpty(text)) {
				frames.AppendEmptyFrame();
			} else {
				frames.Append(Encoding.UTF8.GetBytes(text));
			}
		}

		public static double PopDouble(this NetMQMessage frames) => BitConverter.ToDouble(frames.Pop().Buffer, 0);
		public static void AppendDouble(this NetMQMessage frames, double value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static bool PopBoolean(this NetMQMessage frames) => BitConverter.ToBoolean(frames.Pop().Buffer, 0);
		public static void AppendBoolean(this NetMQMessage frames, bool value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static int PopInt(this NetMQMessage frames) => BitConverter.ToInt32(frames.Pop().Buffer, 0);
		public static void AppendInt(this NetMQMessage frames, int value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static uint PopUInt(this NetMQMessage frames) => BitConverter.ToUInt32(frames.Pop().Buffer, 0);
		public static void AppendUInt(this NetMQMessage frames, uint value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static void AppendInt16(this NetMQMessage frames, short value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static ulong PopULong(this NetMQMessage frames) => BitConverter.ToUInt64(frames.Pop().Buffer, 0);
		public static void AppendULong(this NetMQMessage frames, ulong value)
			=> frames.Append(BitConverter.GetBytes(value));

		public static long PopLong(this NetMQMessage frames) => BitConverter.ToInt64(frames.Pop().Buffer, 0);
		public static void AppendLong(this NetMQMessage frames, long value)
			=> frames.Append(BitConverter.GetBytes(value));
	}
}