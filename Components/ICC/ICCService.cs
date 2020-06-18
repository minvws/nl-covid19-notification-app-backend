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

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public interface IIccService
    {
        Task<InfectionConfirmationCodeEntity> Get(string icc);
        Task<InfectionConfirmationCodeEntity> Validate(string IccodeString);
        Task<InfectionConfirmationCodeEntity> GenerateIcc(Guid userId, bool save = false);
        Task<List<InfectionConfirmationCodeEntity>> GenerateBatch(Guid userId, int count = 20);
        Task<InfectionConfirmationCodeEntity> RedeemIcc(string icc);
    }

    public class IccService : IIccService
    {
        private readonly IccBackendContentDbContext _DbContext;
        private readonly IConfiguration _Configuration;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public IccService(IccBackendContentDbContext dbContext, IConfiguration configuration, IUtcDateTimeProvider dateTimeProvider)
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
        /// <exception cref="IccNotFoundException"></exception>
        public async Task<InfectionConfirmationCodeEntity> Get(string icc)
        {
            InfectionConfirmationCodeEntity Icc = await _DbContext.InfectionConfirmationCodes.FindAsync(icc);
            if (Icc == null) throw new IccNotFoundException();
            return Icc;
        }
        
        /// <summary>
        /// Checks if Icc exists and is valid
        /// </summary>
        /// <param name="IccodeString"></param>
        /// <returns>Icc if valid else null</returns>
        public async Task<InfectionConfirmationCodeEntity> Validate(string IccodeString)
        {
            InfectionConfirmationCodeEntity icc = await Get(IccodeString);
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
        /// Generate Icc with configuration length and A-Z, 0-9 characters
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<InfectionConfirmationCodeEntity> GenerateIcc(Guid userId, bool save = true)
        {
            random = new Random();
            int length = Convert.ToInt32(_Configuration.GetSection("IccConfig:Code:Length").Value);
            string chars = _Configuration.GetSection("IccConfig:Code:Chars").Value;
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
        /// Generate Icc batch with size [count]
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<InfectionConfirmationCodeEntity>> GenerateBatch(Guid userId, int count = 20)
        {
            List<InfectionConfirmationCodeEntity> batch = new List<InfectionConfirmationCodeEntity>();

            for (int i = 0; i < count; i++)
            {
                batch.Add(await GenerateIcc(userId, true));
            }

            await _DbContext.SaveChangesAsync();
            return batch;
        }

        public async Task<InfectionConfirmationCodeEntity> RedeemIcc(string icc)
        {
            InfectionConfirmationCodeEntity Icc = await Get(icc);
            Icc.Used = _DateTimeProvider.Now();
            await _DbContext.SaveChangesAsync();
            return Icc;
        }
    }
    public class IccNotFoundException : Exception
    {
    }
}