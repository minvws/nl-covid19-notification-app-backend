// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using EfgsTestDataGenerator.Models;
using Google.Protobuf;
using Iks.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;

namespace EfgsTestDataGenerator.Services
{
    public class EfgsDataService
    {
        private readonly ILogger<EfgsDataService> _logger;
        private readonly IRandomNumberGenerator _numberGenerator;

        private int NumberOfKeys => 10;
        private List<EfgsDataSet> _batches;

        public EfgsDataService(ILogger<EfgsDataService> logger, IRandomNumberGenerator numberGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));

            GenerateEfgsData();
        }

        public EfgsDataSet GetEfgsDataSet(string date, StringValues batchTag)
        {
            EfgsDataSet efgsDataSet = null;

            if (string.IsNullOrEmpty(batchTag.ToString()))
            {
                efgsDataSet = _batches.FirstOrDefault(p => p.BatchTag.StartsWith(date.Replace("-", "")));
            }
            else
            {
                efgsDataSet = _batches.FirstOrDefault(p => p.BatchTag == batchTag);
            }

            return efgsDataSet;
        }

        private void GenerateEfgsData()
        {
            var dateString = DateTime.UtcNow.Date.AddDays(-7).ToString("yyyyMMdd");

            _batches = new List<EfgsDataSet>
            {
                new EfgsDataSet {
                    BatchTag = $"{dateString}-1",
                    NextBatchTag = $"{dateString}-2",
                    Content = GenerateKeys()
                },
                new EfgsDataSet {
                    BatchTag = $"{dateString}-2",
                    NextBatchTag = $"{dateString}-3",
                    Content = GenerateKeys()
                },
                new EfgsDataSet {
                    BatchTag = $"{dateString}-3",
                    NextBatchTag = string.Empty,
                    Content = GenerateKeys()
                }
            };
        }

        private DiagnosisKeyBatch GenerateKeys()
        {
            var args = new List<InteropKeyFormatterArgs>();
            for (var i = 0; i < NumberOfKeys; i++)
            {
                args.Add(new InteropKeyFormatterArgs
                {
                    CountriesOfInterest = new[] { "BE", "GR", "LT", "PT", "BG", "ES", "LU", "RO", "CZ", "FR", "HU", "SI", "DK", "HR", "MT", "SK", "DE", "IT", "NL", "FI", "EE", "CY", "AT", "SE", "IE", "LV", "PL", "IS", "NO", "LI", "CH" },
                    DaysSinceSymtpomsOnset = 2000,
                    TransmissionRiskLevel = 3,
                    Origin = "DE",
                    Value = new DailyKey
                    {
                        KeyData = _numberGenerator.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                        RollingPeriod = _numberGenerator.Next(1, UniversalConstants.RollingPeriodRange.Hi),
                        RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber()
                    }
                });
            }

            var result = new DiagnosisKeyBatch();
            result.Keys.AddRange(args.Select(Map));

            var succes = TryParse(result.ToByteArray(), out result);

            return result;
        }

        private DiagnosisKey Map(InteropKeyFormatterArgs arg)
        {
            var result = new DiagnosisKey
            {
                KeyData = ByteString.CopyFrom(arg.Value.KeyData),
                RollingPeriod = (uint)arg.Value.RollingPeriod,
                RollingStartIntervalNumber = (uint)arg.Value.RollingStartNumber,
                DaysSinceOnsetOfSymptoms = arg.DaysSinceSymtpomsOnset,
                TransmissionRiskLevel = 3,
                ReportType = Iks.Protobuf.EfgsReportType.ConfirmedTest,
                Origin = arg.Origin,
            };
            result.VisitedCountries.AddRange(arg.CountriesOfInterest);
            return result;
        }

        private bool TryParse(byte[] buffer, out DiagnosisKeyBatch result)
        {
            result = null;
            try
            {
                var parser = new MessageParser<DiagnosisKeyBatch>(() => new DiagnosisKeyBatch());
                result = parser.ParseFrom(buffer);
                return true;
            }
            catch (InvalidProtocolBufferException e)
            {
                _logger.LogError("Error reading IKS protobuf.", e.ToString());
                return false;
            }
        }
    }
}
