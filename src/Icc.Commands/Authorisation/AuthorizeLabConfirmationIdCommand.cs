using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class AuthorizeLabConfirmationIdCommand
    {
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly ILogger _Logger;

        public AuthorizeLabConfirmationIdCommand(WorkflowDbContext workflowDb, ILogger<AuthorisationWriterCommand> logger)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Assume args validated
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteAsync(AuthorisationArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var wf = await _WorkflowDb
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
            {
                _Logger.WriteKeyReleaseWorkflowStateNotFound(args.LabConfirmationId);
                return false;
            }

            _Logger.LogInformation("LabConfirmationId {LabConfirmationId} authorized.", wf.LabConfirmationId);

            return true;
        }
    }
}
