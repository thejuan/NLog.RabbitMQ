using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace NLog.Targets.RabbitMQ.Listener
{
	// SRP violated, I know, I know... But someone please think of the cohesion!!
	internal class Program
	{
		class ParsedOptions : CommandLineOptionsBase
		{
			[Option("h", "hostname", DefaultValue = "localhost", HelpText = "RabbitMQ server IP or DNS name")]
			public string HostName { get; set; }

			[Option("e", "exchange", DefaultValue = "app-logging", HelpText = "What exchange to bind the listener queue to")]
			public string Exchange { get; set; }

			[Option("p", "password", DefaultValue = "guest", HelpText = "Password for authenticating")]
			public string Password { get; set; }

			[Option("u", "username", DefaultValue = "guest", HelpText = "Username for authenticating")]
			public string UserName { get; set; }

			[Option(null, "virtual-host", DefaultValue = "/", HelpText = "VHost to listen to")]
			public string VHost { get; set; }

			[Option(null, "pretty-json", DefaultValue = false, HelpText = "Expect log output to be JSON and try to output it pretty")]
			public bool PrettyJSON { get; set; }

			[Option(null, "basic-props", DefaultValue = false, HelpText = "Also output the IBasicProperties data from the RMQ envelope.")]
			public bool BasicProps { get; set; }

			[Option("d", "durable", DefaultValue = null, HelpText = 
				"Whether to create a durable RMQ queue; it will be there permanently " +
				"until you *close the listener* - WARNING - if you have a busy broker " +
				"you can sink your broker if you don't drain the queue. Sample: " +
				"'-d mylistener'", MutuallyExclusiveSet = "Durabilities")]
			public string Durable { get; set; }

			[Option("D", "permanently-durable", DefaultValue = null, HelpText =
				"Whether to create a durable RMQ queue; it will be there permanently " +
				"until you **delete it manually** - WARNING - if you have a busy " +
				"broker you can sink your broker if you don't drain the queue. Sample: " +
				"'-D mylistener'", MutuallyExclusiveSet = "Durabilities")]
			public string PermDurable { get; set; }

			[HelpOption]
			public string GetUsage()
			{
				return @"
Usage: nlog-rmq-listener [options]
Description: Will create an adjacent file 'log.txt' and also output all log data to the console.

" + HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
			}

			public ConnectionFactory Factory
			{
				get
				{
					return new ConnectionFactory
						{
							HostName = HostName,
							UserName = UserName,
							Password = Password,
							VirtualHost = VHost,
							Protocol = Protocols.DefaultProtocol
						};
				}
			}
		}

		static ParsedOptions ParseArgsToFactory(string[] args)
		{
			var opts = new ParsedOptions();
			var parser = new CommandLineParser(new CommandLineParserSettings(true, true));

			if (!parser.ParseArguments(args, opts))
			{
				Console.WriteLine(opts.GetUsage());
				Environment.Exit(1);
			}

			return opts;
		}

		private static readonly object _locker = new object();
		static volatile bool _stopping;

		private static void Main(string[] args)
		{
			var factory = ParseArgsToFactory(args);
			Console.WriteLine("Hello.");
			Console.CancelKeyPress += Console_CancelKeyPress;
			var p = new Program();
			p.ReceiveLoop(factory);
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Goodbye.");
			_stopping = true;
			e.Cancel = false; // let me kill myself instead of you killing me
		}

		void ReceiveLoop(ParsedOptions opts)
		{
			while (!_stopping)
			{
				try
				{
					while (!_stopping)
					{
						try
						{
							Console.WriteLine("Setting up connection...");
							using (var c = opts.Factory.CreateConnection())
							using (var m = c.CreateModel())
							{
								var consumer = new QueueingBasicConsumer(m);
								var props = new Dictionary<string, object>
									{
										{"x-expires", 30*60000} // expire queue after 30 minutes, see http://www.rabbitmq.com/extensions.html
									};

								m.ExchangeDeclarePassive(opts.Exchange);

								var q = m.QueueDeclare("", false, true, false, props); // consuming queue, autogen name
								m.QueueBind(q, opts.Exchange, "#");
								m.BasicConsume(q, true, consumer);

								Console.WriteLine("Entering print loop...");
								PrintLoop(consumer, opts);
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
						opts.Exchange));
				}
			}
		}

		void PrintLoop(QueueingBasicConsumer consumer, ParsedOptions opts)
		{
			while (!_stopping)
			{
				object tmp;
				if (!consumer.Queue.Dequeue(200, out tmp))
					continue;

				var msg = (BasicDeliverEventArgs) tmp;

				if (opts.BasicProps)
					DebugUtil.DumpProperties(msg.BasicProperties, Console.Out, 1);

				var input = msg.Body.AsUtf8String();
				string output;
				try
				{
					output = opts.PrettyJSON
					             	? JsonConvert.SerializeObject(
					             		JsonConvert.DeserializeObject(input),
					             		Formatting.Indented)
					             	: input;
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);
					output = input;
				}

				Console.WriteLine(output);
				WriteToFile(output);
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
