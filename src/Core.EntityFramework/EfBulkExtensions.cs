// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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
        public static async Task BulkDeleteSqlRawAsync<T>(
            this DbContext db,
            string columnName,
            bool checkValue
        )
        {
            var tableName = db.Model.FindEntityType(typeof(T)).GetTableName();
            await db.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName} WHERE {columnName} = {checkValue}");
        }

        public static async Task BulkDeleteSqlRawAsync<T>(
            this DbContext db,
            string ids
        )
        {
            var tableName = db.Model.FindEntityType(typeof(T)).GetTableName();
            await db.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName} WHERE id in ({ids})");
        }

        public static async Task BulkUpdateSqlRawAsync<T>(
            this DbContext db,
            string columnName,
            bool value,
            string ids)
        {
            var tableName = db.Model.FindEntityType(typeof(T)).GetTableName();
            await db.Database.ExecuteSqlRawAsync($"UPDATE {tableName} SET {columnName} = {value} WHERE id in ({ids})");
        }

        public static async Task BulkUpdateSqlRawAsync<T>(
            this DbContext db,
            string columnName,
            int value,
            string ids)
        {
            var tableName = db.Model.FindEntityType(typeof(T)).GetTableName();
            await db.Database.ExecuteSqlRawAsync($"UPDATE {tableName} SET {columnName} = {value} WHERE id in ({ids})");
        }

        public static void BulkInsertBinaryCopy(
            this DbContext db,
            IEnumerable<EksCreateJobInputEntity> entities)
        {
            var (connection, closeConnectionInternally) = OpenAndGetNpgsqlConnection(db);

            var tableName = db.Model.FindEntityType(typeof(EksCreateJobInputEntity)).GetTableName();

            var commaSeparatedColumns =
                "tek_id, used, key_data, rolling_start_number, rolling_period, " +
                "transmission_risk_level, days_since_symptoms_onset, symptomatic, " +
                "report_type";

            using var writer = connection.BeginBinaryImport($"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);");

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
                writer.Write((int)entity.TransmissionRiskLevel, NpgsqlDbType.Integer);
                writer.Write(entity.DaysSinceSymptomsOnset, NpgsqlDbType.Integer);
                writer.Write((int)entity.Symptomatic, NpgsqlDbType.Integer);
                writer.Write(entity.ReportType, NpgsqlDbType.Integer);
            }

            writer.Complete();

            if (closeConnectionInternally)
            {
                connection.Close();
            }
        }

        public static void BulkInsertBinaryCopy(
            this DbContext db,
            IEnumerable<IksCreateJobInputEntity> entities)
        {
            var (connection, closeConnectionInternally) = OpenAndGetNpgsqlConnection(db);

            var tableName = db.Model.FindEntityType(typeof(IksCreateJobInputEntity)).GetTableName();

            var commaSeparatedColumns =
                "countries_of_interest, days_since_symptoms_onset, dk_id, " +
                "report_type, transmission_risk_level, used, daily_key_key_data, " +
                "daily_key_rolling_start_number, daily_key_rolling_period";

            using var writer = connection.BeginBinaryImport($"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);");

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

            if (closeConnectionInternally)
            {
                connection.Close();
            }
        }

        public static void BulkInsertBinaryCopy(this DbContext db, IEnumerable<DiagnosisKeyInputEntity> entities)
        {
            var (connection, closeConnectionInternally) = OpenAndGetNpgsqlConnection(db);

            var tableName = db.Model.FindEntityType(typeof(DiagnosisKeyInputEntity)).GetTableName();

            var commaSeparatedColumns =
                "tek_id, daily_key_key_data, daily_key_rolling_start_number, " +
                "daily_key_rolling_period, local_transmission_risk_level, " +
                "local_days_since_symptoms_onset, local_symptomatic, local_report_type";

            using var writer = connection.BeginBinaryImport($"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);");

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

            if (closeConnectionInternally)
            {
                connection.Close();
            }
        }

        public static void BulkInsertBinaryCopy(this DbContext db, IEnumerable<DiagnosisKeyEntity> entities)
        {
            var (connection, closeConnectionInternally) = OpenAndGetNpgsqlConnection(db);

            var tableName = db.Model.FindEntityType(typeof(DiagnosisKeyEntity)).GetTableName();

            var commaSeparatedColumns =
                "created, origin, published_locally, published_to_efgs, " +
                "ready_for_cleanup, daily_key_key_data, daily_key_rolling_start_number, " +
                "daily_key_rolling_period, efgs_countries_of_interest, " +
                "efgs_days_since_symptoms_onset, efgs_report_type, efgs_country_of_origin, " +
                "local_transmission_risk_level, local_days_since_symptoms_onset, " +
                "local_symptomatic, local_report_type";

            using var writer = connection.BeginBinaryImport($"COPY {tableName} ({commaSeparatedColumns}) FROM STDIN (FORMAT BINARY);");

            foreach (var entity in entities)
            {
                writer.StartRow();

                writer.Write(entity.Created, NpgsqlDbType.TimestampTz);
                writer.Write((int)entity.Origin, NpgsqlDbType.Integer);
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

                writer.Write((int)entity.Local.Symptomatic, NpgsqlDbType.Integer);
                writer.Write(entity.Local.ReportType, NpgsqlDbType.Integer);
            }

            writer.Complete();

            if (closeConnectionInternally)
            {
                connection.Close();
            }
        }

        // From https://github.com/borisdj/EFCore.BulkExtensions/blob/ffc84d1eec1c700d11f39c94e73f4fca63a70948/EFCore.BulkExtensions/SQLAdapters/PostgreSql/PostgreSqlAdapter.cs#L322
        internal static (NpgsqlConnection, bool) OpenAndGetNpgsqlConnection(DbContext context)
        {
            var closeConnectionInternally = false;
            var connection = context.Database.GetDbConnection() as NpgsqlConnection;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                closeConnectionInternally = true;
            }

            return (connection, closeConnectionInternally);
        }
    }
}
