using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.RegisterSecret
{
    public class RegisterSecretLoggingExtensions
    {
        private const string Name = "Register";
        private const int Base = LoggingCodex.Register;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int Writing = Base + 1;
        private const int Committed = Base + 2;
        private const int DuplicatesFound = Base + 3;

        private const int MaximumCreateAttemptsReached = Base + 4;
        private const int Failed = Base + 5;

        private readonly ILogger _Logger;

        public RegisterSecretLoggingExtensions(ILogger<RegisterSecretLoggingExtensions> logger)
		{
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStartSecret()
        {   
            _Logger.LogInformation("[{name}/{id}] POST register triggered.",
                Name, Start);
        }

        public void WriteFailed(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            _Logger.LogCritical(ex, "[{name}/{id}] Failed to create an enrollment response.",
                Name, Failed);
        }
        
        public void WriteWritingStart()
        {
            _Logger.LogDebug("[{name}/{id}] Writing.",
                Name, Writing);
        }
        
        public void WriteDuplicatesFound(int attemptCount)
        {
            _Logger.LogWarning("[{name}/{id}] Duplicates found while creating workflow - Attempt:{AttemptCount}",
                Name, DuplicatesFound,
                attemptCount);
        }
        
        public void WriteMaximumCreateAttemptsReached()
        {
            _Logger.LogCritical("[{name}/{id}] Maximum attempts made at creating workflow.",
                Name, MaximumCreateAttemptsReached);
        }

        public void WriteCommitted()
        {
            _Logger.LogDebug("[{name}/{id}] Committed.",
                Name, Committed);
        }

        public void WriteFinished()
        {
            _Logger.LogDebug("[{name}/{id}] Finished.",
                Name, Finished);
        }
    }
}
