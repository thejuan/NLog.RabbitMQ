using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Linq;

namespace NLog.Targets.RabbitMQ.Listener
{
	// SRP violated, I know, I know... But someone please think of the cohesion!!
	internal class Program
	{
		class ParsedOptions
		{
			public ParsedOptions(string exchangeName, ConnectionFactory factory)
			{
				ExchangeName = exchangeName;
				Factory = factory;
			}

			public string ExchangeName { get; private set; }
			public ConnectionFactory Factory { get; private set; }
		}

		private static readonly object _locker = new object();
		static volatile bool _stopping;

		private static void Main(string[] args)
		{
			if (CheckIfHelp(args)) return;
			Console.WriteLine("Hello.");
			var factory = ParseArgsToFactory(args);
			Console.CancelKeyPress += Console_CancelKeyPress;
			var p = new Program();
			p.ReceiveLoop(factory);
		}

		static bool CheckIfHelp(string[] args)
		{
			var helpItems = new HashSet<string>
				{
					"-h",
					"/?",
					"--help",
					"help"
				};

			if (args.Length != 0 && args.Any(helpItems.Contains))
			{
				Console.WriteLine(
@"Usage: nlog-rmq-listener [server] [username] [password] [vhost] [exchange]

Description:
	Will create an adjacent file 'log.txt' and also output all log data to the console.

Optional Parameters:
	server		RabbitMQ server IP or DNS name
				Default '/'

	username	Username for authenticating
				Default 'guest'

	password	Password for authenticating
				Default 'guest'

	vhost		VHost to listen to
				Default '/'
				
	exchange	What exchange to bind the listener queue to
				Default 'app-logging'

Usage: nlog-rmq-listener [/?|-h|--help|help]

Description:
	Will display this help. So you'd better not name your server 'help'!

");
				return true;
			}
			return false;
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Goodbye.");
			_stopping = true;
			e.Cancel = false; // let me kill myself instead of you killing me
		}

		static ParsedOptions ParseArgsToFactory(string[] args)
		{
			var fac = new ConnectionFactory
				{
					HostName = At(args, 0, "localhost"),
					UserName = At(args, 1, "guest"),
					Password = At(args, 2, "guest"),
					VirtualHost = At(args, 3, "/"),
					Protocol = Protocols.DefaultProtocol
				};

			var exchange = At(args, 4, "app-logging");

			return new ParsedOptions(exchange, fac);
		}

		static string At(string[] args, int index, string def)
		{
			if (index >= args.Length) return def;
			return args[index];
		}

		void ReceiveLoop(ParsedOptions opts)
		{
			while (!_stopping)
			{
				try
				{
					while (true)
					{
						try
						{
							using (var c = opts.Factory.CreateConnection())
							using (var m = c.CreateModel())
							{
								var consumer = new QueueingBasicConsumer(m);
								var props = new Dictionary<string, object>
									{
										{"x-expires", 30*60000} // expire queue after 30 minutes, see http://www.rabbitmq.com/extensions.html
									};

								m.ExchangeDeclarePassive(opts.ExchangeName);

								var q = m.QueueDeclare("", false, true, false, props); // consuming queue, autogen name
								m.QueueBind(q, opts.ExchangeName, "#");
								m.BasicConsume(q, true, consumer);

								PrintLoop(consumer);
							}
						}
						catch (BrokerUnreachableException)
						{
							Console.WriteLine("Could not connect, retrying...");
							Thread.Sleep(1000);
						}
					}
				}
				// sometimes this happens if rabbit is shut down
				catch (EndOfStreamException)
				{
					Console.WriteLine("RabbitMQ went down, re-connecting...");
				}
				// this happens most of the time
				catch (IOException)
				{
					Console.WriteLine("RabbitMQ went down ungracefully, re-connecting...");
				}
				// this happens when one kills erlang (killall -9 erl)
				catch (OperationInterruptedException)
				{
					Console.WriteLine(string.Format("Yet another one of RabbitMQ's failure modes - re-connecting... (you might not have the {0} exchange in your broker)",
						opts.ExchangeName));
				}
			}
		}

		void PrintLoop(QueueingBasicConsumer consumer)
		{
			while (!_stopping)
			{
				object tmp;
				if (!consumer.Queue.Dequeue(200, out tmp))
					continue;

				var msg = (BasicDeliverEventArgs) tmp;

				if (msg.BasicProperties.IsAppIdPresent())
					Console.Write(msg.BasicProperties.AppId + " ");

				if (!String.IsNullOrEmpty(msg.RoutingKey))
					Console.Write(msg.RoutingKey + " ");

				var asUtf8String = msg.Body.AsUtf8String();

				Console.WriteLine(asUtf8String);
				WriteToFile(asUtf8String);
			}
		}

		private static void WriteToFile(string body)
		{
			lock (_locker)
				File.AppendAllLines("log.txt", new[] {body});
		}
	}

	static class Extensions
	{
		public static string AsUtf8String(this byte[] args)
		{
			return Encoding.UTF8.GetString(args);
		}
	}
}
