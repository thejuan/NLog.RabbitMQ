// Copyright 2012 Henrik Feldt

using System;
using Newtonsoft.Json;

namespace NLog.Targets
{
	public sealed class LogLine
	{
		[JsonProperty("timeStampISO8601")]
		public string TimeStampISO8601 { get; set; }

		[JsonProperty("fullMessage")]
		public string FullMessage { get; set; }

		[JsonProperty("exception")]
		public Exception Exception { get; set; }

		[JsonProperty("hostName")]
		public string HostName { get; set; }
	}
}