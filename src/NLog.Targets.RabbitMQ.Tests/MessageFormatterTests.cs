// Copyright 2012 Henrik Feldt

using System;
using System.Globalization;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;
using Newtonsoft.Json;

namespace NLog.RabbitMQ.Tests
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
				null,
				GenerateException());

			evt.Properties.Add("tags", new[] { "skurk:rånarligan" });
		}

		Exception GenerateException()
		{
			try
			{
				var a = 0;
				var c = 1/a;
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
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
			var json = LogLine();
			Assert.That(json.Message, Is.EqualTo("Hello World"));
		}

		LogLine LogLine()
		{
			var res = MessageFormatter.GetMessageInner(true, l, evt);
			Console.WriteLine(res);
			var json = JsonConvert.DeserializeObject<LogLine>(res);
			return json;
		}

		[Test]
		public void contains_exception()
		{
			var res = MessageFormatter.GetMessageInner(true, l, evt);
			Assert.That(res, Is.StringContaining(@"""exception"":"));
		}

		[Test]
		public void level_is_debug()
		{
			var json = LogLine();
			Assert.That(json.Level, Is.EqualTo("Debug"));
		}

		[Test]
		public void source_scheme_is_nlog()
		{
			var json = LogLine();
			Assert.That(json.Source.Scheme, Is.EqualTo("nlog"));
		}

		[Test]
		public void contains_tags()
		{
			var json = LogLine();
			CollectionAssert.AreEqual(new[]{"skurk:rånarligan"}, json.Tags);
		}
        
        [Test]
		public void contains_custom_properties()
		{
            evt.Properties.Add("requestId", "This request");
			var json = LogLine();
            Assert.AreEqual("This request", json.Fields["requestId"]);
		}
        
        [Test]
		public void ignores_custom_properties_with_non_string_keys()
		{
            evt.Properties.Add(42, "Some value");
			var json = LogLine();
            CollectionAssert.DoesNotContain(json.Fields.Values, "Some value");
		}
	}
}