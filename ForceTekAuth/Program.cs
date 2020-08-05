// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace ForceTekAuth
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                new ConsoleAppRunner().Execute(args, Configure, Start);
                return 0;
            }
            catch(Exception)
            {
                return -1;
            }
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<ForceTekAuthCommand>().Execute(args);
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => DbContextStartup.Workflow(x, false));
            services.AddTransient<ForceTekAuthCommand>();
        }
    }

    internal class ForceTekAuthCommand
    {
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly IUtcDateTimeProvider _Dtp;

        public ForceTekAuthCommand(WorkflowDbContext workflowDb, IUtcDateTimeProvider dtp)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
        }

        public void Execute(string[] _)
        {
            ForceWorkflowAuth();

            using var tx = _WorkflowDb.BeginTransaction();

            Console.WriteLine($"Authorised.");

            var teks = _WorkflowDb.TemporaryExposureKeys
                .Include(x => x.Owner)
                .Where(x => x.PublishingState == PublishingState.Unpublished && x.PublishAfter > _Dtp.Snapshot)
                .ToList();

            foreach (var i in teks)
            {
                i.PublishAfter = _Dtp.Snapshot;
                _WorkflowDb.TemporaryExposureKeys.Update(i);
            }
            
            _WorkflowDb.SaveAndCommit();
        }

        private void ForceWorkflowAuth()
        {
            using var tx = _WorkflowDb.BeginTransaction();

            var notAuthed = _WorkflowDb.KeyReleaseWorkflowStates
                .Where(x => x.AuthorisedByCaregiver == null)
                .ToList();

            Console.WriteLine($"Found: {notAuthed.Count}.");

            foreach (var i in notAuthed)
            {
                i.AuthorisedByCaregiver = _Dtp.Snapshot;
                i.DateOfSymptomsOnset = _Dtp.Snapshot.Date.AddDays(-1);
                i.LabConfirmationId = null;
            }

            _WorkflowDb.BulkUpdate(notAuthed);
            _WorkflowDb.SaveAndCommit();
        }
    }
}
