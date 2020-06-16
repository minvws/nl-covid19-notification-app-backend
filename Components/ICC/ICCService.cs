// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC
{
    public interface IICCService
    {
        Task<InfectionConfirmationCodeEntity> Get(string icc);
        Task<InfectionConfirmationCodeEntity> Validate(string ICCodeString);
        Task<InfectionConfirmationCodeEntity> GenerateICC(Guid userId, bool save = false);
        Task<List<InfectionConfirmationCodeEntity>> GenerateBatch(Guid userId, int count = 20);
        Task<InfectionConfirmationCodeEntity> RedeemICC(string icc);
    }

    public class ICCService : IICCService
    {
        private readonly ICCBackendContentDbContext _DbContext;
        private readonly IConfiguration _Configuration;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public ICCService(ICCBackendContentDbContext dbContext, IConfiguration configuration, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContext = dbContext;
            _Configuration = configuration;
            _DateTimeProvider = dateTimeProvider;
        }

        /// <summary>
        /// Get InfectionConfirmationCodeEntity by raw icc string
        /// </summary>
        /// <param name="icc"></param>
        /// <returns></returns>
        /// <exception cref="ICCNotFoundException"></exception>
        public async Task<InfectionConfirmationCodeEntity> Get(string icc)
        {
            InfectionConfirmationCodeEntity ICC = await _DbContext.InfectionConfirmationCodes.FindAsync(icc);
            if (ICC == null) throw new ICCNotFoundException();
            return ICC;
        }
        
        /// <summary>
        /// Checks if ICC exists and is valid
        /// </summary>
        /// <param name="ICCodeString"></param>
        /// <returns>ICC if valid else null</returns>
        public async Task<InfectionConfirmationCodeEntity> Validate(string ICCodeString)
        {
            InfectionConfirmationCodeEntity icc = await Get(ICCodeString);
            if (icc != null && icc.IsValid()) return icc;
            return null;
        }

        private static Random random;
        private static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        
        /// <summary>
        /// Generate ICC with configuration length and A-Z, 0-9 characters
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<InfectionConfirmationCodeEntity> GenerateICC(Guid userId, bool save = true)
        {
            random = new Random();
            int length = Convert.ToInt32(_Configuration.GetSection("ICCConfig:Code:Length").Value);
            string chars = _Configuration.GetSection("ICCConfig:Code:Chars").Value;
            string generatedIcc = RandomString(length, chars);

            InfectionConfirmationCodeEntity icc = new InfectionConfirmationCodeEntity();
            icc.Code = generatedIcc;
            icc.GeneratedBy = userId;
            icc.Created = _DateTimeProvider.Now();


            _DbContext.InfectionConfirmationCodes.Add(icc);
            if (save) await _DbContext.SaveChangesAsync();
            return icc;
        }

        /// <summary>
        /// Generate ICC batch with size [count]
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<InfectionConfirmationCodeEntity>> GenerateBatch(Guid userId, int count = 20)
        {
            List<InfectionConfirmationCodeEntity> batch = new List<InfectionConfirmationCodeEntity>();

            for (int i = 0; i < count; i++)
            {
                batch.Add(await GenerateICC(userId, true));
            }

            await _DbContext.SaveChangesAsync();
            return batch;
        }

        public async Task<InfectionConfirmationCodeEntity> RedeemICC(string icc)
        {
            InfectionConfirmationCodeEntity ICC = await Get(icc);
            ICC.Used = _DateTimeProvider.Now();
            await _DbContext.SaveChangesAsync();
            return ICC;
        }
    }
    public class ICCNotFoundException : Exception
    {
    }
}