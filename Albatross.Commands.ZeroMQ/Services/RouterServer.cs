using Albatross.Collections;
using Albatross.Commands.ZeroMQ.Configurations;
using Albatross.Commands.ZeroMQ.Messages;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Albatross.Commands.ZeroMQ.Services {
	public class RouterServer : IMessagingService, IDisposable {
		private readonly RouterServerConfiguration config;
		private readonly RouterSocket socket;
		private readonly NetMQPoller poller;
		private readonly NetMQQueue<object> queue;
		private readonly NetMQTimer timer;
		private readonly ILogger<RouterServer> logger;
		private readonly IMessageFactory messageFactory;
		private bool running = false;
		private bool disposed = false;
		private IEnumerable<IRouterServerService> receiveServices;
		private IEnumerable<IRouterServerService> transmitServices;
		private IEnumerable<IRouterServerService> timerServices;
		private Dictionary<string, Client> clients = new Dictionary<string, Client>();
		private IAtomicCounter counter;
		private ulong timerCounter;

		public IAtomicCounter Counter => this.counter;

		public RouterServer(RouterServerConfiguration config, IEnumerable<IRouterServerService> services, ILoggerFactory loggerFactory, IMessageFactory messageFactory) {
			this.logger = loggerFactory.CreateLogger<RouterServer>();
			logger.LogInformation("Creating {name} instance w. maintainConnection={maintain}, receiveHighWatermark={rchw}, sendHighWatermark={sendhw}, timerInterval={timerintv}",
				nameof(RouterServer), config.MaintainConnection, config.ReceiveHighWatermark, config.SendHighWatermark, config.ActualTimerInterval);
			this.config = config;
			this.receiveServices = services.Where(args => args.CanReceive).ToArray();
			this.transmitServices = services.Where(args => args.HasCustomTransmitObject).ToArray();
			this.timerServices = services.Where(args => args.NeedTimer).ToArray();
			this.messageFactory = messageFactory;
			this.counter = new DurableAtomicCounter(config.DiskStorage.WorkingDirectory);
			this.socket = new RouterSocket();
			if (config.UseCurveEncryption) {
				if(string.IsNullOrEmpty(config.ServerPrivateKey)) {
					throw new ArgumentException("curve encryption is enabled but the server private key is missing");
				}
				socket.Options.CurveServer = true;
				socket.Options.CurveCertificate = NetMQCertificate.CreateFromSecretKey(config.ServerPrivateKey);
			}
			this.socket.ReceiveReady += Socket_ReceiveReady;
			this.socket.Options.ReceiveHighWatermark = config.ReceiveHighWatermark;
			this.socket.Options.SendHighWatermark = config.SendHighWatermark;
			this.queue = new NetMQQueue<object>();
			this.queue.ReceiveReady += Queue_ReceiveReady;
			this.timer = new NetMQTimer(config.ActualTimerInterval);
			this.timer.Elapsed += Timer_Elapsed;
			this.poller = new NetMQPoller { socket, queue, };
			if (timerServices.Any() || config.MaintainConnection) {
				this.poller.Add(timer);
			} else {
				timer.Enable = false;
			}
		}

		private void Timer_Elapsed(object? sender, NetMQTimerEventArgs e) {
			timerCounter++;
			if (config.MaintainConnection) {
				foreach (var client in this.clients.Values) {
					if (client.State != ClientState.Dead) {
						var elapsed = DateTime.UtcNow - client.LastHeartbeat;
						if (elapsed > config.HeartbeatThresholdTimeSpan) {
							client.Lost();
							logger.LogInformation("lost: {name}, {elapsed:#,#} > {threshold:#,#}", client.Identity, elapsed.TotalMilliseconds, config.HeartbeatThresholdTimeSpan.TotalMilliseconds);
						}
					}
				}
			}
			foreach (var service in this.timerServices) {
				try {
					service.ProcessTimerElapsed(this, timerCounter);
				} catch (Exception ex) {
					logger.LogError(ex, "error processing timer elapsed from {type}", service.GetType().FullName);
				}
			}
		}

		private void Queue_ReceiveReady(object? sender, NetMQQueueEventArgs<object> e) {
			try {
				if (running) {
					var item = e.Queue.Dequeue();
					switch (item) {
						case IMessage msg:
							this.Transmit(msg);
							break;
						default:
							foreach (var service in this.transmitServices) {
								if (service.ProcessQueue(this, item)) {
									return;
								}
							}
							if (!(item is ISystemMessage)) {
								logger.LogError("transmit queue item {@item} not processed", item);
							}
							break;
					}
				} else {
					logger.LogError("cannot process transmit queue item because the router server is being disposed");
				}
			} catch (Exception err) {
				logger.LogError(err, "error processing command reply");
			}
		}

		private void Socket_ReceiveReady(object? sender, NetMQSocketEventArgs e) {
			try {
				var frames = e.Socket.ReceiveMultipartMessage();
				var msg = this.messageFactory.Create(frames);
				if (msg is UnknownMsg) {
					logger.LogError("unknown message: {@msg}", msg);
					return;
				}
				if (msg is ClientAck) { return; }
				if (running) {
					if (msg is Connect connect) {
						AcceptConnection(connect);
					} else if (msg is Heartbeat heartbeat) {
						AcceptHeartbeat(heartbeat);
						return;
					}
					foreach (var service in receiveServices) {
						if (service.ProcessReceivedMsg(this, msg)) {
							return;
						}
					}
					if (!(msg is ISystemMessage)) {
						logger.LogInformation("unhandled router server msg: {msg}", msg);
					}
				} else {
					logger.LogError("incoming message {msg} cannot get processed because the router server is being disposed", msg);
				}
			} catch (Exception err) {
				logger.LogError(err, "error parsing router server message");
			}
		}

		private void AcceptConnection(Connect msg) {
			var client = clients.GetOrAdd(msg.Route, () => {
				logger.LogInformation("new client: {name}", msg.Route);
				return new Client(msg.Route);
			});
			this.Transmit(new ConnectOk(msg.Route, counter.NextId()));
			if (client.State == ClientState.Dead) {
				client.Connected();
				SendClientResumeMsgToService(client.Identity);
			}
		}

		private void SendClientResumeMsgToService(string client) {
			logger.LogInformation("client {name} came back from lost, sending resume signal", client);
			foreach (var service in receiveServices) {
				service.ProcessReceivedMsg(this, new Resume(client, counter.NextId()));
			}
		}

		private void AcceptHeartbeat(Heartbeat heartbeat) {
			if (clients.TryGetValue(heartbeat.Route, out var client)) {
				client.UpdateHeartbeat();
				Transmit(new HeartbeatAck(heartbeat.Route, counter.NextId()));
				if (client.State != ClientState.Alive) {
					SendClientResumeMsgToService(client.Identity);
				}
			} else {
				// receive a heartbeat from non existing client.  asking the client to reconnect
				this.Transmit(new Reconnect(heartbeat.Route, counter.NextId()));
			}
		}

		public void Start() {
			if (!running) {
				running = true;
				this.logger.LogInformation("starting router server at {endpoint}", config.EndPoint);
				this.socket.Bind(config.EndPoint);
				// wait a second here.  if we start transmitting messages right away, it will get lost
				Task.Delay(1000).Wait();
				this.poller.RunAsync();
			}
		}

		public void SubmitToQueue(object result) => this.queue.Enqueue(result);

		public void Transmit(IMessage msg) {
			var frames = msg.Create();
			this.socket.SendMultipartMessage(frames);
		}

		public ClientState GetClientState(string identity) {
			if (config.MaintainConnection) {
				if (clients.TryGetValue(identity, out var client)) {
					return client.State;
				} else {
					return ClientState.Dead;
				}
			} else {
				return ClientState.Alive;
			}
		}
		public void Dispose() {
			if (!disposed) {
				running = false;
				logger.LogInformation("closing and disposing router server");
				poller.Stop();
				poller.RemoveAndDispose(socket);
				poller.Dispose();
				queue.Dispose();
				disposed = true;
				logger.LogInformation("router server disposed");
			}
		}
	}
}