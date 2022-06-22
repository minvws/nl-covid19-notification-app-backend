// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using Npgsql;
using NpgsqlTypes;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public static class EfBulkExtensions
    {
        public static async Task BulkUpdateWithTransactionAsync<T>(
            this DbContext db,
            IList<T> page,
            SubsetBulkArgs args) where T : class
        {
            await using (db.BeginTransaction())
            {
                await db.BulkUpdateAsync(page, args.ToBulkConfig());
                db.SaveAndCommit();
            }
        }

        // Temporarily disabled BulkInsertWithTransactionAsync method until EFCore.BulkExtensions
        // adds support for bulk inserting Owned entity types in PostgreSQL.
        // Replaced with BulkCopy methods below.

        // public static async Task BulkInsertWithTransactionAsync<T>(this DbContext db, IList<T> page, SubsetBulkArgs args) where T : class
        // {
        //     await using (db.BeginTransaction())
        //     {
        //         await db.BulkInsertAsync(page, args.ToBulkConfig());
        //         db.SaveAndCommit();
        //     }
        // }

        public static async Task BulkDeleteWithTransactionAsync<T>(
            this DbContext db,
            IList<T> page,
            SubsetBulkArgs args) where T : class
        {
            await using (db.BeginTransaction())
            {
                await db.BulkDeleteAsync(page, args.ToBulkConfig());
                db.SaveAndCommit();
            }
        }

        // Temporary (hard-coded) BulkCopy methods to replace
        // BulkInsertWithTransactionAsync method above
        public static void BulkCopyEksIn(
            this DbContext db,
            IEnumerable<EksCreateJobInputEntity> entities)
        {
            var connection = db.Database.GetDbConnection() as NpgsqlConnection;
            connection.Open();

            var tableName = "\"EksCreateJobInput\"";

            var commaSeparatedColumns =
                "\"TekId\", \"KeyData\", \"RollingStartNumber\", \"RollingPeriod\", " +
                "\"TransmissionRiskLevel\", \"DaysSinceSymptomsOnset\", \"Symptomatic\", " +
                "\"ReportType\"";

            var sqlCopy =
                $"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);";

            using var writer = connection.BeginBinaryImport(sqlCopy);

            foreach (var entity in entities)
            {
                writer.StartRow();

                if (entity.TekId.HasValue)
                {
                    writer.Write(entity.TekId.Value, NpgsqlDbType.Bigint);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.Write(entity.Used, NpgsqlDbType.Boolean);
                writer.Write(entity.KeyData, NpgsqlDbType.Bytea);
                writer.Write(entity.RollingStartNumber, NpgsqlDbType.Integer);
                writer.Write(entity.RollingPeriod, NpgsqlDbType.Integer);
                writer.Write(entity.TransmissionRiskLevel, NpgsqlDbType.Integer);
                writer.Write(entity.DaysSinceSymptomsOnset, NpgsqlDbType.Integer);
                writer.Write(entity.Symptomatic, NpgsqlDbType.Integer);
                writer.Write(entity.ReportType, NpgsqlDbType.Integer);
            }

            writer.Complete();
            connection.Close();
        }

        public static void BulkCopyIksIn(
            this DbContext db,
            IEnumerable<IksCreateJobInputEntity> entities)
        {
            var connection = db.Database.GetDbConnection() as NpgsqlConnection;
            connection.Open();

            var tableName = "\"IksCreateJobInput\"";

            var commaSeparatedColumns =
                "\"CountriesOfInterest\", \"DaysSinceSymptomsOnset\", \"DkId\", " +
                "\"ReportType\", \"TransmissionRiskLevel\", \"Used\", \"DailyKey_KeyData\", " +
                "\"DailyKey_RollingStartNumber\", \"DailyKey_RollingPeriod\"";

            var sqlCopy =
                $"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);";

            using var writer = connection.BeginBinaryImport(sqlCopy);

            foreach (var entity in entities)
            {
                writer.StartRow();
                writer.Write(entity.CountriesOfInterest, NpgsqlDbType.Text);
                writer.Write(entity.DaysSinceSymptomsOnset, NpgsqlDbType.Integer);
                writer.Write(entity.DkId, NpgsqlDbType.Bigint);
                writer.Write((int)entity.ReportType, NpgsqlDbType.Integer);
                writer.Write((int)entity.TransmissionRiskLevel, NpgsqlDbType.Integer);
                writer.Write(entity.Used, NpgsqlDbType.Boolean);
                writer.Write(entity.DailyKey.KeyData, NpgsqlDbType.Bytea);
                writer.Write(entity.DailyKey.RollingStartNumber, NpgsqlDbType.Integer);
                writer.Write(entity.DailyKey.RollingPeriod, NpgsqlDbType.Integer);
            }

            writer.Complete();
            connection.Close();
        }

        public static void BulkCopyDkIn(this DbContext db, IEnumerable<DiagnosisKeyInputEntity> entities)
        {
            var connection = db.Database.GetDbConnection() as NpgsqlConnection;
            connection.Open();

            var tableName = "\"DiagnosisKeysInput\"";

            var commaSeparatedColumns =
                "\"TekId\", \"DailyKey_KeyData\", \"DailyKey_RollingStartNumber\", " +
                "\"DailyKey_RollingPeriod\", \"Local_TransmissionRiskLevel\", " +
                "\"Local_DaysSinceSymptomsOnset\", \"Local_Symptomatic\", \"Local_ReportType\"";

            var sqlCopy =
                $"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);";

            using var writer = connection.BeginBinaryImport(sqlCopy);

            foreach (var entity in entities)
            {
                writer.StartRow();
                writer.Write(entity.TekId, NpgsqlDbType.Bigint);
                writer.Write(entity.DailyKey.KeyData, NpgsqlDbType.Bytea);
                writer.Write(entity.DailyKey.RollingStartNumber, NpgsqlDbType.Integer);
                writer.Write(entity.DailyKey.RollingPeriod, NpgsqlDbType.Integer);

                if (entity.Local.TransmissionRiskLevel.HasValue)
                {
                    writer.Write((int)entity.Local.TransmissionRiskLevel.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                if (entity.Local.DaysSinceSymptomsOnset.HasValue)
                {
                    writer.Write(entity.Local.DaysSinceSymptomsOnset.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.Write((int)entity.Local.Symptomatic, NpgsqlDbType.Integer);
                writer.Write(entity.Local.ReportType, NpgsqlDbType.Integer);
            }

            writer.Complete();
            connection.Close();
        }

        public static void BulkCopyDk(this DbContext db, IEnumerable<DiagnosisKeyEntity> entities)
        {
            var connection = db.Database.GetDbConnection() as NpgsqlConnection;
            connection.Open();

            var tableName = "\"DiagnosisKeys\"";

            var commaSeparatedColumns =
                "\"Created\", \"Origin\", \"PublishedLocally\", \"PublishedToEfgs\", " +
                "\"ReadyForCleanup\", \"DailyKey_KeyData\", \"DailyKey_RollingStartNumber\", " +
                "\"DailyKey_RollingPeriod\", \"Efgs_CountriesOfInterest\", " +
                "\"Efgs_DaysSinceSymptomsOnset\", \"Efgs_ReportType\", \"Efgs_CountryOfOrigin\", " +
                "\"Local_TransmissionRiskLevel\", \"Local_DaysSinceSymptomsOnset\", " +
                "\"Local_Symptomatic\", \"Local_ReportType\"";

            var sqlCopy =
                $"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);";

            using var writer = connection.BeginBinaryImport(sqlCopy);

            foreach (var entity in entities)
            {
                writer.StartRow();

                writer.Write(entity.Created, NpgsqlDbType.TimestampTz);
                writer.Write(entity.Origin, NpgsqlDbType.Integer);
                writer.Write(entity.PublishedLocally, NpgsqlDbType.Boolean);
                writer.Write(entity.PublishedToEfgs, NpgsqlDbType.Boolean);

                if (entity.ReadyForCleanup.HasValue)
                {
                    writer.Write(entity.ReadyForCleanup.Value, NpgsqlDbType.Boolean);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.Write(entity.DailyKey.KeyData, NpgsqlDbType.Bytea);
                writer.Write(entity.DailyKey.RollingStartNumber, NpgsqlDbType.Integer);
                writer.Write(entity.DailyKey.RollingPeriod, NpgsqlDbType.Integer);
                writer.Write(entity.Efgs.CountriesOfInterest, NpgsqlDbType.Text);

                if (entity.Efgs.DaysSinceSymptomsOnset.HasValue)
                {
                    writer.Write(entity.Efgs.DaysSinceSymptomsOnset.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                if (entity.Efgs.ReportType.HasValue)
                {
                    writer.Write((int)entity.Efgs.ReportType.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.Write(entity.Efgs.CountryOfOrigin, NpgsqlDbType.Text);

                if (entity.Local.TransmissionRiskLevel.HasValue)
                {
                    writer.Write((int)entity.Local.TransmissionRiskLevel.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                if (entity.Local.DaysSinceSymptomsOnset.HasValue)
                {
                    writer.Write(entity.Local.DaysSinceSymptomsOnset.Value, NpgsqlDbType.Integer);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.Write(entity.Local.Symptomatic, NpgsqlDbType.Integer);
                writer.Write(entity.Local.ReportType, NpgsqlDbType.Integer);
            }

            writer.Complete();
            connection.Close();
        }
    }
}
