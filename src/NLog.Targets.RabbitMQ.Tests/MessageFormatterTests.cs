// Copyright 2012 Henrik Feldt

using System.Globalization;
using NLog.Layouts;
using NUnit.Framework;
using Newtonsoft.Json;

namespace NLog.Targets.RabbitMQ.Tests
{
	public class MessageFormatterTests
	{
		Layout l;
		LogEventInfo evt;

		[SetUp]
		public void given()
		{
			l = "${message}";
			evt = new LogEventInfo(
				LogLevel.Debug,
				"MessageFormatterTests",
				CultureInfo.InvariantCulture,
				"Hello World",
				null);
		}

		/// <summary><see cref="LogLine"/></summary>
		[Test]
		public void contains_iso8601_timestamp()
		{
			var res = MessageFormatter.GetMessageInner(true, l, evt);
			Assert.That(res, Is.StringContaining(
				evt.TimeStamp.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)));
		}

		[Test]
		public void contains_message()
		{
			var res = MessageFormatter.GetMessageInner(true, l, evt);
			dynamic json = JsonConvert.DeserializeObject(res);
			Assert.That(json.fullMessage.ToString(), Is.EqualTo("Hello World"));
		}

		[Test]
		public void contains_no_exception()
		{
			var res = MessageFormatter.GetMessageInner(true, l, evt);
			Assert.That(res, Is.StringContaining(@"""exception"":null"));
		}
	}
}