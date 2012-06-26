// Copyright 2012 Henrik Feldt

using System;
using Newtonsoft.Json;

namespace NLog.Targets
{
	public sealed class LogLine
	{
		[JsonProperty("timestamp")]
		public string TimeStampISO8601 { get; set; }

		[JsonProperty("message")]
		public string FullMessage { get; set; }

		[JsonProperty("exception")]
		public Exception Exception { get; set; }

		[JsonProperty("host")]
		public string HostName { get; set; }
	}
}