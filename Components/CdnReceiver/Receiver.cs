using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class Receiver<T> where T : ContentEntity, new()
    {
        public async Task<IActionResult> Execute(HttpRequest httpRequest, ILogger logger, IConfiguration configuration)
        {
            var content = Serializer.Deserialize<ContentArgs>(httpRequest.Body);

            //TODO check sig!!!

            if (content == null)
                return new OkResult();

            var e = new T
            {
                PublishingId = content.PublishingId,
                SignedContent = content.Content,
                SignedContentTypeName = MediaTypeNames.Application.Zip,
                Release = content.Released,
            };

            var config = new StandardEfDbConfig(configuration, "Content");
            var builder = new SqlServerDbContextOptionsBuilder(config);

            var db = new ExposureContentDbContext(builder.Build());
            try

            {
                db.BeginTransaction();
                await db.Set<T>().AddAsync(e);
                db.SaveAndCommit();
            }
            catch (SqlException ex)
            {
                if (ex.Errors.AsQueryable().Cast<SqlError>().Select(x => x.Number).Contains(2627))
                    return new ConflictResult();

                throw;
            }

            return new OkResult();
        }
    }
}