// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using Serilog.Extensions.Logging;

namespace IccBackend.Tests
{
    [TestClass]
    public class LabVerifyTests 
    {
        private DbConnection _Connection;
        private WorkflowDbContext _DbContext;
        private FakeTimeProvider _FakeTimeProvider;
        private ILoggerFactory _Lf;
        private FakePollTokenService _PollTokenService;
        private HttpPostLabVerifyCommand _Command;
        private long _BucketPk;

        private class FakeTimeProvider : IUtcDateTimeProvider
        {
            public DateTime Value { get; set; }
            public DateTime Now() => Value;
            public DateTime Snapshot => Value;
        }

        [TestInitialize]
        public async Task InitializeAsync()
        {
            _Connection = new SqliteConnection("Data Source=:memory:");
            _DbContext = new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_Connection).Options);
            _Lf = new SerilogLoggerFactory();
            _FakeTimeProvider = new FakeTimeProvider();
            _PollTokenService = new FakePollTokenService();
            _Command = new HttpPostLabVerifyCommand(
                new LabVerifyArgsValidator(_PollTokenService),
                _DbContext,
                _Lf.CreateLogger<HttpPostLabVerifyCommand>(),
                new WriteNewPollTokenWriter(_DbContext, _PollTokenService, _Lf.CreateLogger<WriteNewPollTokenWriter>())
            );

            await _Connection.OpenAsync();
            await _DbContext.Database.EnsureCreatedAsync();

        }

        private async Task WriteBucket(string pollToken)
        {
            var now = _FakeTimeProvider.Snapshot;

            var e = new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[32],
                ValidUntil = now.AddHours(1),
                Created = now,
                ConfirmationKey = new byte[32],
                AuthorisedByCaregiver = now,
                DateOfSymptomsOnset = now.AddDays(-2),
                TeksTouched = false,
                PollToken = pollToken
            };
            _DbContext.KeyReleaseWorkflowStates.Add(e);
            await _DbContext.SaveChangesAsync();
            _BucketPk = e.Id;
        }

        private void PostTeks()
        {
            var b = _DbContext.KeyReleaseWorkflowStates.Find(_BucketPk);
            b.TeksTouched = true;
            _DbContext.SaveChanges();
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await _DbContext.DisposeAsync();
            await _Connection.CloseAsync();
            await _Connection.DisposeAsync();
        }

        private class FakePollTokenService : IPollTokenService
        {
            private int _Value;
            public string Next() => (++_Value).ToString();
            public bool ValidateToken(string value) => true;
        }

        [TestMethod]
        public async Task NoTekPost()
        {
            // Arrange
            _FakeTimeProvider.Value = new DateTime(2020, 8, 2, 0, 0, 0, DateTimeKind.Utc);
            var bucket = new byte[32];
            var pollToken = _PollTokenService.Next();
            await WriteBucket(pollToken);

            // Act
            var args = new LabVerifyArgs { PollToken = pollToken };
            _DbContext.BeginTransaction();
            var result = _Command.Execute(args);

            // Assert
            Assert.AreEqual(false, ((LabVerifyAuthorisationResponse)((OkObjectResult)result.Result).Value).Valid);
            var wf = _DbContext.KeyReleaseWorkflowStates.Single(x => x.BucketId == bucket);
            Assert.AreEqual(0, wf.Teks.Count);
            Assert.AreEqual(false, wf.TeksTouched);
        }

        [TestMethod]
        public async Task WorkflowMiss()
        {
            // Arrange
            _FakeTimeProvider.Value = new DateTime(2020, 8, 2, 0, 0, 0, DateTimeKind.Utc);
            var pollToken = _PollTokenService.Next();
            await WriteBucket(pollToken);

            // Act
            var args = new LabVerifyArgs { PollToken = "Missed!" };
            _DbContext.BeginTransaction();
            var result = _Command.Execute(args);

            // Assert
            var response = ((LabVerifyAuthorisationResponse)((OkObjectResult)result.Result).Value);
            Assert.AreEqual("Workflow not found.", response.Error);
            Assert.AreEqual(false, response.Valid);
        }

        [TestMethod]
        public async Task TeksPosted()
        {
            // Arrange
            _FakeTimeProvider.Value = new DateTime(2020, 8, 2, 0, 0, 0, DateTimeKind.Utc); //Touch
            var pollToken = _PollTokenService.Next();
            await WriteBucket(pollToken);
            PostTeks();

            // Act
            var args = new LabVerifyArgs { PollToken = pollToken };
            _DbContext.BeginTransaction();
            var result = _Command.Execute(args);

            // Assert
            var response = (LabVerifyAuthorisationResponse)((OkObjectResult)result.Result).Value;
            Assert.AreEqual(true, response.Valid);
        }
    }
}
