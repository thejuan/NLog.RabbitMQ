using System;
using System.Globalization;
using System.Net;
using NLog.Layouts;
using Newtonsoft.Json;

namespace NLog.Targets
{
	public static class MessageFormatter
	{
		private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		static string _hostName;
		private static string HostName
		{
			get { return _hostName = (_hostName ?? Dns.GetHostName()); }
		}

		public static string GetMessageInner(bool useJSON, Layout layout, LogEventInfo info)
		{
			if (!useJSON)
				return layout.Render(info);

			return JsonConvert.SerializeObject(new LogLine
				{
					TimeStampISO8601 = 
						info.TimeStamp.ToUniversalTime()
						.ToString("o", CultureInfo.InvariantCulture),
					FullMessage = info.FormattedMessage,
					Exception = info.Exception,
					HostName = HostName
				});
		}

		public static long GetEpochTimeStamp(LogEventInfo @event)
		{
			return Convert.ToInt64((@event.TimeStamp - _epoch).TotalSeconds);
		}
	}
}