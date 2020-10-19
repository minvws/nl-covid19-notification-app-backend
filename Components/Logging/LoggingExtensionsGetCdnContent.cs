using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.GetCdnContent
{
	public static class LoggingExtensionsGetCdnContent
	{
		public static void WriteInvalidType(this ILogger logger, string cdnId)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Invalid generic content type - {Id}.",
				LoggingDataGetCdnContent.Name, LoggingDataGetCdnContent.InvalidType,
				cdnId);
		}

		public static void WriteInvalidId(this ILogger logger, string cdnId)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Invalid content id - {Id}.",
				LoggingDataGetCdnContent.Name, LoggingDataGetCdnContent.InvalidId,
				cndId);
		}

		public static void WriteHeaderMissing(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Required request header missing - if-none-match.",
				LoggingDataGetCdnContent.Name, LoggingDataGetCdnContent.HeaderMissing
				);
		}

		public static void WriteNotFound(this ILogger logger, string cdnId)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Content not found - {Id}.",
				LoggingDataGetCdnContent.Name, LoggingDataGetCdnContent.NotFound,
				cdnId);
		}

		public static void WriteEtagFound(this ILogger logger, string cdnId)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Matching etag found, responding with 304 - {Id}.",
				LoggingDataGetCdnContent.Name, LoggingDataGetCdnContent.EtagFound,
				cdnId);
		}
	}
}
