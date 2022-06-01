// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests
{
    public class NewTeksFilter15Tests
    {
        private class FakedNow : IUtcDateTimeProvider
        {
            private readonly DateTime _fixed;
            public FakedNow(DateTime @fixed)
            {
                _fixed = @fixed;
            }
            public DateTime Snapshot => _fixed;
            public DateTime Now() => _fixed;
        }

        private readonly List<TekReleaseWorkflowStateEntity> _workflows;

        private DateTime _t;

        private DateTime T
        {
            get => _t;
            set
            {
                if (value <= _t)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _t = value;
            }
        }

        public NewTeksFilter15Tests()
        {
            _workflows = new List<TekReleaseWorkflowStateEntity>();
        }

        private Tek[] Publish(DateTime runTime)
        {
            var authorised = _workflows
                .Where(x => x.AuthorisedByCaregiver != null)
                .ToArray();

            var result = authorised
                .SelectMany(x => x.Teks)
                .Where(x => x.PublishingState == PublishingState.Unpublished && x.PublishAfter <= runTime)
                .ToArray();

            foreach (var i in result)
            {
                i.PublishingState = PublishingState.Published;
            }

            return result.Select(Mapper.MapToTek).ToArray();
        }

        private void Write(TekReleaseWorkflowStateEntity workflow, Tek[] arts)
        {
            if (!_workflows.Contains(workflow))
            {
                _workflows.Add(workflow);
            }

            foreach (var i in arts.Select(Mapper.MapToEntity))
            {
                workflow.Teks.Add(i);
            }
        }

        Tek GenerateTek(int year, int month, int day, int q, int rollingPeriod = 144)
        {
            var keyData = BitConverter.GetBytes(day * 100 + q);
            var t = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc).Date.ToRollingStartNumber();
            return new Tek { RollingStartNumber = t, KeyData = keyData, RollingPeriod = rollingPeriod };
        }

        [Fact]
        public void Gaen15SameDay_UsingStaticDateTime_TekReleaseOn()
        {
            //Sep 1-14
            var deviceTeks = Enumerable.Range(1, 14).Select(x => GenerateTek(2020, 9, x, 1)).ToList();

            var w = new TekReleaseWorkflowStateEntity
            {
                Created = new DateTime(2020, 9, 14, 10, 0, 0),
                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
            };
            Write(w, new Tek[0]);
            var fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 14, 9, 35, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(14, fr.Items.Length);
            Write(w, fr.Items); //These will be lost cos they don't get GGD authorisation
            Assert.Empty(Publish(new DateTime(2020, 9, 14, 9, 36, 0)));

            deviceTeks.Add(GenerateTek(2020, 9, 14, 2));

            //Sep 15
            deviceTeks.RemoveAt(0);
            deviceTeks.Add(GenerateTek(2020, 9, 15, 1)); //1st key for today
            Assert.Equal(15, deviceTeks.Count);


            w = new TekReleaseWorkflowStateEntity
            {
                Created = new DateTime(2020, 9, 14, 10, 0, 0),
                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
            };
            Write(w, new Tek[0]);

            deviceTeks.Add(GenerateTek(2020, 9, 15, 2)); //2nd key for today
            Assert.Equal(16, deviceTeks.Count);
            Assert.Empty(Publish(new DateTime(2020, 9, 14, 9, 36, 0)));

            //Post
            w.AuthorisedByCaregiver = new DateTime(2020, 9, 15, 11, 0, 0);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 15, 11, 5, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(2, fr.Messages.Length);
            Assert.Equal(14, fr.Items.Length);
            Write(w, fr.Items);
            //Get after post
            deviceTeks.Add(GenerateTek(2020, 9, 15, 3));
            Assert.Equal(17, deviceTeks.Count);

            //11:20 Server publishes Server publishes K0902.1 through K0915.2 to the CDN. 
            //Only 14.
            Assert.Equal(14, Publish(new DateTime(2020, 9, 15, 13, 6, 0)).Length);
            Assert.Equal(14, w.Teks.Count(x => x.PublishingState == PublishingState.Published));

            //14.00h - POST - Server silently discards all keys as they arrive > 120 minutes after GGD code
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 15, 14, 0, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Empty(fr.Items);
            Assert.Equal(17, fr.Messages.Length); //14.00h Server silently discards all keys as they arrive > 120 minutes after GGD code

            deviceTeks.Add(GenerateTek(2020, 9, 15, 4));
            Assert.Equal(18, deviceTeks.Count);

            Assert.Empty(Publish(new DateTime(2020, 9, 15, 23, 59, 0)));

            //Sep 16
            deviceTeks.RemoveAt(0);
            deviceTeks.Add(GenerateTek(2020, 9, 16, 1));
            Assert.Equal(18, deviceTeks.Count);
            //Nothing new to publish
            Assert.Empty(Publish(new DateTime(2020, 9, 16, 0, 1, 0)));

            //POST
            // - ignores the keys it already has
            // - K0916.1 is discarded because it's a key for today and the bucket doesn't accept same day keys after midnight.
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 16, 0, 30, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Empty(fr.Items);
            Assert.Equal(18, fr.Messages.Length);
            Assert.Empty(Publish(new DateTime(2020, 9, 16, 0, 31, 0)));
        }

        [Fact]
        public void Gaen15SameDay_UsingCurrentDateTime_TekReleaseOn_Publishing()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            // -14 days to today
            var deviceTeks = Enumerable.Range(0, 14).Select(x => GenerateTek(today.AddDays(-x).Year, today.AddDays(-x).Month, today.AddDays(-x).Day, 1)).ToList();

            var w = new TekReleaseWorkflowStateEntity
            {
                Created = today.AddHours(10),
                ValidUntil = today.AddDays(1).AddHours(4),
            };
            Write(w, new Tek[0]);
            var filteredResult = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(today.AddHours(9).AddMinutes(35)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(14, filteredResult.Items.Length);
            Write(w, filteredResult.Items); //These will be lost cos they don't get GGD authorisation
            Assert.Empty(Publish(today.AddHours(9).AddMinutes(36)));

            deviceTeks.Add(GenerateTek(today.Year, today.Month, today.Day, 2));

            // Tomorrow
            deviceTeks.RemoveAt(0);
            deviceTeks.Add(GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 1)); //1st key for today
            Assert.Equal(15, deviceTeks.Count);


            w = new TekReleaseWorkflowStateEntity
            {
                Created = today.AddHours(10),
                ValidUntil = today.AddDays(1).AddHours(4),
            };
            Write(w, new Tek[0]);

            deviceTeks.Add(GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 2)); //2nd key for today
            Assert.Equal(16, deviceTeks.Count);
            Assert.Empty(Publish(today.AddHours(9).AddMinutes(36)));

            //Post
            w.AuthorisedByCaregiver = today.AddDays(1).AddHours(11);
            filteredResult = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(today.AddDays(1).AddHours(11).AddMinutes(5)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(2, filteredResult.Messages.Length);
            Assert.Equal(14, filteredResult.Items.Length);
            Write(w, filteredResult.Items);
            //Get after post
            deviceTeks.Add(GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 3));
            Assert.Equal(17, deviceTeks.Count);

            //11:20 Server publishes Server publishes K0902.1 through K0915.2 to the CDN. 
            //Only 14.
            Assert.Equal(14, Publish(today.AddDays(1).AddHours(13).AddMinutes(6)).Length);
            Assert.Equal(14, w.Teks.Count(x => x.PublishingState == PublishingState.Published));

            //14.00h - POST - Server silently discards all keys as they arrive > 120 minutes after GGD code
            filteredResult = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(today.AddDays(1).AddHours(14)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Empty(filteredResult.Items);
            Assert.Equal(17, filteredResult.Messages.Length); //14.00h Server silently discards all keys as they arrive > 120 minutes after GGD code

            deviceTeks.Add(GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 4));
            Assert.Equal(18, deviceTeks.Count);

            Assert.Empty(Publish(today.AddDays(1).AddHours(23).AddMinutes(59)));

            // Day after tomorrow
            deviceTeks.RemoveAt(0);
            var dayAfterTomorrow = DateTime.UtcNow.Date.AddDays(2);
            deviceTeks.Add(GenerateTek(dayAfterTomorrow.Year, dayAfterTomorrow.Month, dayAfterTomorrow.Day, 1));
            Assert.Equal(18, deviceTeks.Count);
            //Nothing new to publish
            Assert.Empty(Publish(today.AddDays(2).AddMinutes(1)));

            //POST
            // - ignores the keys it already has
            // - K0916.1 is discarded because it's a key for today and the bucket doesn't accept same day keys after midnight.
            filteredResult = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(today.AddDays(2).AddMinutes(30)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Empty(filteredResult.Items);
            Assert.Equal(18, filteredResult.Messages.Length);
            Assert.Empty(Publish(today.AddDays(2).AddMinutes(31)));
        }

        [Fact]
        public void Gaen15SameDay_UsingCurrentDateTime_TekRelease_Has_CorrectEmbargoTime()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // add keys from yesterday, today and tomorrow
            var deviceTeks = new List<Tek>
            {
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 1, 1),
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 2, 57),
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 3, 58),
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 4, 69),
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 5, 70),
                GenerateTek(yesterday.Year, yesterday.Month, yesterday.Day, 6, 144),
                GenerateTek(today.Year, today.Month, today.Day, 7, 1),
                GenerateTek(today.Year, today.Month, today.Day, 8, 57), // 9:30 AM UTC (before embargo  time)
                GenerateTek(today.Year, today.Month, today.Day, 9, 58), // 9:40 AM UTC (before embargo  time)
                GenerateTek(today.Year, today.Month, today.Day, 10, 69), // 11:30 AM UTC (before embargo  time)
                GenerateTek(today.Year, today.Month, today.Day, 11, 70), // 11:40 AM UTC (after embargo  time)
                GenerateTek(today.Year, today.Month, today.Day, 12, 144),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 13, 1),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 14, 57),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 15, 58),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 16, 69),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 17, 70),
                GenerateTek(tomorrow.Year, tomorrow.Month, tomorrow.Day, 18, 144)
            };


            var w = new TekReleaseWorkflowStateEntity
            {
                Created = today.AddHours(10),
                ValidUntil = today.AddDays(1).AddHours(4),
            };
            Write(w, new Tek[0]);

            // Assume it's 9.35 AM UTC
            var todaysDateTime = today.AddHours(9).AddMinutes(35);

            var filteredResult = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(todaysDateTime), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(12, filteredResult.Items.Length); // Yesterdays and todays keys. Future keys are filtered out
            Write(w, filteredResult.Items);
            Assert.Equal(6, _workflows.First().Teks.Count(p => p.PublishAfter == DateTime.MinValue)); // Yesterdays keys have no PublishAfter date 
            Assert.Equal(4, _workflows.First().Teks.Count(p => p.PublishAfter == todaysDateTime.AddHours(2))); // 4 keys have 2 hours embargo time
            Assert.Equal(2, _workflows.First().Teks.Count(p => p.PublishAfter == todaysDateTime.Date.AddDays(1).AddHours(2))); // 2 keys have next day + 2 hours embargo time
        }

        [Fact]
        public void Gaen14Or15_SameDay_TekReleaseOff_NoPublishing()
        {
            var keysOnDevice = Enumerable.Range(1, 14).Select(x => GenerateTek(2020, 9, x, 1)).ToList();

            T = new DateTime(2020, 9, 14, 10, 0, 0, DateTimeKind.Utc);
            var w = new TekReleaseWorkflowStateEntity
            {
                Created = T,
                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
            };
            Write(w, new Tek[0]);

            //Hold back todays key(s)???
            var send = keysOnDevice.Where(x => x.End <= T.Date.ToRollingStartNumber()).ToArray();
            var fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(send, w);
            Assert.Empty(fr.Messages);
            Assert.Equal(13, fr.Items.Length);
            Write(w, fr.Items);
            Assert.Equal(14, keysOnDevice.Count);

            T = new DateTime(2020, 9, 14, 23, 59, 0, DateTimeKind.Utc);
            Assert.Empty(Publish(T));

            //Sep 15
            keysOnDevice.RemoveAt(0);
            var lastOfSet = keysOnDevice.Last(); //Careful - this only works for a single key on the previous day.
            keysOnDevice.Add(GenerateTek(2020, 9, 15, 1));
            Assert.Equal(14, keysOnDevice.Count);

            //Sep 15 00:30
            T = new DateTime(2020, 9, 15, 0, 30, 0, DateTimeKind.Utc);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(new[] { lastOfSet }, w);
            Assert.Single(fr.Items);
            Assert.Empty(fr.Messages);
            Write(w, fr.Items);

            T = new DateTime(2020, 9, 15, 9, 59, 0, DateTimeKind.Utc);
            //Still not published
            Assert.Empty(Publish(T));

            //Sep 15 10:00
            T = new DateTime(2020, 9, 15, 10, 0, 0, DateTimeKind.Utc);
            w = new TekReleaseWorkflowStateEntity
            {
                Created = T,
                ValidUntil = new DateTime(2020, 9, 16, 4, 0, 0, DateTimeKind.Utc),
            };

            //Sep 15 11:00
            T = new DateTime(2020, 9, 15, 11, 0, 0, DateTimeKind.Utc);
            w.AuthorisedByCaregiver = T;
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(keysOnDevice.Take(keysOnDevice.Count - 1).ToArray(), w);
            Assert.Empty(fr.Messages);
            Write(w, fr.Items);
            Assert.Equal(keysOnDevice.Count - 1, w.Teks.Count);

            //Sep 15 11:20 Server publishes K0902.1 through K0915.2 to the CDN.
            T = new DateTime(2020, 9, 15, 13, 1, 0, DateTimeKind.Utc);
            Assert.Equal(13, Publish(T).Length);

            //Sausage fingers again - but later than the document cos Publish delayed
            T = new DateTime(2020, 9, 15, 13, 30, 0, DateTimeKind.Utc);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(keysOnDevice.Take(keysOnDevice.Count - 1).ToArray(), w);
            Assert.Empty(fr.Items);
            Assert.Equal(13, fr.Messages.Length);
            Assert.Equal(14, keysOnDevice.Count);

            T = new DateTime(2020, 9, 15, 23, 59, 0, DateTimeKind.Utc);
            Assert.Empty(Publish(T));

            //Sep 16 11:00
            keysOnDevice.RemoveAt(0);
            keysOnDevice.Add(GenerateTek(2020, 9, 16, 1));
            Assert.Equal(14, keysOnDevice.Count);
            //The nightly batch uploads all keys, including now K0915.1 to bucket B ???

            T = new DateTime(2020, 9, 16, 0, 30, 0, DateTimeKind.Utc);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(keysOnDevice.ToArray(), w);
            Assert.Single(fr.Items);
            Assert.Equal(13, fr.Messages.Length);
            Write(w, fr.Items);

            T = new DateTime(2020, 9, 16, 2, 30, 0, DateTimeKind.Utc);
            Assert.Single(Publish(T));
        }
    }
}
