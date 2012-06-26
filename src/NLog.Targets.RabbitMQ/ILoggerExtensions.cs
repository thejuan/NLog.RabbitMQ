// Copyright 2012 Henrik Feldt
namespace NLog
{
	public static class ILoggerExtensions
	{
		public static void FatalTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Fatal);
		}
		public static void ErrorTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Error);
		}
		public static void WarnTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Warn);
		}
		public static void InfoTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Info);
		}
		public static void DebugTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Debug);
		}
		public static void TraceTag(this Logger logger, string message, params string[] tags)
		{
			Log(logger, message, tags, LogLevel.Trace);
		}

		static void Log(Logger logger, string message, string[] tags, LogLevel level)
		{
			if (message == null) return;
			var info = new LogEventInfo(level, logger.Name, message);
			if (tags != null) info.Properties.Add("tags", tags);
			logger.Log(typeof (ILoggerExtensions), info);
		}
	}
}