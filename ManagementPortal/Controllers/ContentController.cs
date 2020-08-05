// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementPortal.Controllers
{
    public class ContentController : Controller
    {
        private readonly ContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly ContentDbContext _Context;

        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public ContentController(ContentValidator validator, ContentInsertDbCommand insertDbCommand, ContentDbContext context, IUtcDateTimeProvider dateTimeProvider)
        {
            _Validator = validator;
            _InsertDbCommand = insertDbCommand;
            _Context = context;
            _DateTimeProvider = dateTimeProvider;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET
        [HttpGet]
        public IActionResult List()
        {
            var dateTimeSnapshot = _DateTimeProvider.Snapshot;

            var items = _Context.Content.ToList();
            ViewData["items"] = items;
            return View();
        }

        [HttpPost, Route("content")]
        public async Task<IActionResult> PostContent([FromForm] ContentArgs contentArgs)
        {
            if (!_Validator.IsValid(contentArgs)) return BadRequest("Invalid Json");

            using var tx = _Context.Database.BeginTransaction();
            await _InsertDbCommand.Execute(contentArgs);
            _Context.SaveAndCommit();

            return new OkObjectResult(contentArgs);
        }
    }
}