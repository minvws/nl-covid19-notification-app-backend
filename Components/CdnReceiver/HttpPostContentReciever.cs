using System;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using ProtoBuf;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class HttpPostContentReciever<T> where T : ContentEntity, new()
    {
        private readonly ExposureContentDbContext _DbContext;

        public HttpPostContentReciever(ExposureContentDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<IActionResult> Execute(BinaryContentResponse content)
        {
            //TODO check sig!!!

            if (content == null)
                return new OkResult();

            var e = new T
            {
                PublishingId = content.PublishingId,
                Content = content.Content,
                ContentTypeName = content.ContentTypeName,
                SignedContent = content.SignedContent,
                SignedContentTypeName = MediaTypeNames.Application.Zip,
                Release = content.LastModified,
            };

            try
            {
                await _DbContext.Set<T>().AddAsync(e);
                _DbContext.SaveAndCommit();
            }
            catch (DbUpdateException ex)
            {
                if ((ex?.InnerException as SqlException)?.Number == 2601)
                    return new ConflictResult();

                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return new OkResult();
        }

    }
}