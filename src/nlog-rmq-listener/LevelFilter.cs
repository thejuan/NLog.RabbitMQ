// Copyright 2012 Henrik Feldt

using System;
using Newtonsoft.Json;

namespace NLog.RabbitMQ.Listener
{
	public static class LevelFilter
	{
		public static bool ShouldPrint(string sample, LogLevel filterLevel, out string formatted)
		{
			LogLevel lvl;
			try
			{
				dynamic o = JsonConvert.DeserializeObject(sample);
				lvl = LogLevel.FromString((string)o.level);
				formatted = JsonConvert.SerializeObject(o, Formatting.Indented);
			}
			catch (Exception)
			{
				lvl = LogLevel.Trace;
				formatted = sample;
			}
			return filterLevel <= lvl;
		}
	}
}