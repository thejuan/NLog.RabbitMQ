// Copyright 2012 Henrik Feldt

using NLog.RabbitMQ.Listener;
using NUnit.Framework;
using Newtonsoft.Json;

namespace NLog.RabbitMQ.Tests
{
	public class LevelFilterTests
	{
		string sample =
@"{
  ""@source"": ""nlog://isomorphism/NetDialog.One2Many.PL.ChatAdmin.Global"",
  ""@timestamp"": ""2012-06-28T15:06:40.3219373Z"",
  ""@message"": ""starting application"",
  ""@fields"": {},
  ""@tags"": [
    ""app"",
    ""lifecycle""
  ],
  ""level"": ""Info""
}";

		[Test]
		public void can_read_level()
		{
			dynamic o = JsonConvert.DeserializeObject(sample);
			Assert.That((string)o.level, Is.EqualTo("Info"));
		}

		[Test]
		public void should_print_if_input_higher()
		{
			string formatted;
			var res = LevelFilter.ShouldPrint(sample, LogLevel.Info, out formatted);
			Assert.That(res, Is.True);
		}

		[Test]
		public void should_print_if_higher_2()
		{
			string formatted;
			var res = LevelFilter.ShouldPrint(sample, LogLevel.Trace, out formatted);
			Assert.That(res, Is.True);
		}

		[Test]
		public void should_not_print_if_lower()
		{
			string formatted;
			Assert.That(
				LevelFilter.ShouldPrint(sample, LogLevel.Warn, out formatted),
				Is.False);
		}
	}
}