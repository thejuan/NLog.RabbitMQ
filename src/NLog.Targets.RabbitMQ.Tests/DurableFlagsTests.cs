// Copyright 2012 Henrik Feldt

using System;
using NLog.Targets.RabbitMQ.Listener;
using NUnit.Framework;

namespace NLog.Targets.RabbitMQ.Tests
{
	public class DurableFlagsTests
	{
		Func<string> notTransient = () =>
			{
				Assert.Fail("should not call this");
				return "4"; // from dice, random
			};

		[Test]
		public void Durable_flags_failure_mutual_exclusitivity()
		{
			Assert.That(
				Program.ParseArgsToFactory("-D", "my-queue", "-d", "other-q"),
				Is.Null);
		}

		[Test]
		public void Durable_flags_final_queue_name()
		{
			var parse = Program.ParseArgsToFactory("-D", "my-q");
			
			parse.WithQueue(notTransient, Assert.Fail, qn => Assert.That(qn, Is.EqualTo("my-q")));
		}

		[Test]
		public void Durable_flag_should_not_be_deleted()
		{
			var parse = Program.ParseArgsToFactory("-D", "qq");
			parse.WithQueue(notTransient, Assert.Fail, Assert.Pass);
		}

		[Test]
		public void small_durable_flag_should_be_deleted_after_run()
		{
			var parse = Program.ParseArgsToFactory("-d", "qq");
			parse.WithQueue(notTransient, Assert.Pass, Assert.Fail);
		}

		[Test]
		public void no_durable_flag_means_transient_q()
		{
			var parse = Program.ParseArgsToFactory(new string[0]);
			parse.WithQueue(() =>
				{
					Assert.Pass("OK!");
					return "5";
				}, Assert.Fail, Assert.Fail);
		}
	}
}