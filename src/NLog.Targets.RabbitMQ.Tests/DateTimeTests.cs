using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace NLog.Targets.RabbitMQ.Tests
{
	public class DateTimeTests
	{
		[Test]
		public void when_outputting_with_s_and_o()
		{
			var swedishCulture = new CultureInfo("sv-SE");
			var british = new CultureInfo("en-GB");
			var french = new CultureInfo("fr-FR");
			
			foreach (var c in new[]{"s", "o"}) {
				Console.WriteLine("{0}, swe: {1}", c, DateTime.UtcNow.ToString(c, swedishCulture));
				Console.WriteLine("{0}, br: {1}", c, DateTime.UtcNow.ToString(c, british));
				Console.WriteLine("{0}, fr: {1}", c, DateTime.UtcNow.ToString(c, french));
				Console.WriteLine("{0}, invariant: {1}", c, DateTime.UtcNow.ToString(c, CultureInfo.InvariantCulture));
			}
		}
	}
}
