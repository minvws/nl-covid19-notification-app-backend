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
            private readonly DateTime _Fixed;
            public FakedNow(DateTime @fixed)
            {
                _Fixed = @fixed;
            }
            public DateTime Snapshot => _Fixed;
            //ncrunch: no coverage start
            public DateTime Now() => _Fixed;

            //ncrunch: no coverage end
        }

        private List<TekReleaseWorkflowStateEntity> _Workflows;

        private DateTime _T;

        private DateTime T
        {
            get => _T;
            set
            {
                if (value <= _T) throw new ArgumentOutOfRangeException();
                _T = value;
            }
        }

        public NewTeksFilter15Tests()
        {
            _Workflows = new List<TekReleaseWorkflowStateEntity>();
        }

        private Tek[] Publish(DateTime runTime)
        {
            var authorised = _Workflows
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
            if (!_Workflows.Contains(workflow))
                _Workflows.Add(workflow);

            foreach (var i in arts.Select(Mapper.MapToEntity))
                workflow.Teks.Add(i);
        }

        Tek GenerateTek(int m, int d, int q)
        {
            var keyData = BitConverter.GetBytes(d * 100 + q);
            var t = new DateTime(2020, m, d, 0, 0, 0, DateTimeKind.Utc).Date.ToRollingStartNumber();
            return new Tek {RollingStartNumber = t, KeyData = keyData, RollingPeriod = UniversalConstants.RollingPeriodRange.Hi};
        }

        [Fact]
        public void Gaen15SameDayTekReleaseOn()
        {
            //Sep 1-14
            var deviceTeks = Enumerable.Range(1, 14).Select(x => GenerateTek(9, x, 1)).ToList();
            
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

            deviceTeks.Add(GenerateTek(9, 14, 2));

            //Sep 15
            deviceTeks.RemoveAt(0);
            deviceTeks.Add(GenerateTek(9, 15, 1)); //1st key for today
            Assert.Equal(15, deviceTeks.Count);


            w = new TekReleaseWorkflowStateEntity
            {
                Created = new DateTime(2020, 9, 14, 10, 0, 0),
                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
            };
            Write(w, new Tek[0]);

            deviceTeks.Add(GenerateTek(9, 15, 2)); //2nd key for today
            Assert.Equal(16, deviceTeks.Count);
            Assert.Empty(Publish(new DateTime(2020, 9, 14, 9, 36, 0)));

            //Post
            w.AuthorisedByCaregiver = new DateTime(2020, 9, 15, 11, 0, 0);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 15, 11, 5, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Equal(2, fr.Messages.Length);
            Assert.Equal(14, fr.Items.Length);
            Write(w, fr.Items);
            //Get after post
            deviceTeks.Add(GenerateTek(9, 15, 3));
            Assert.Equal(17, deviceTeks.Count);

            //11:20 Server publishes Server publishes K0902.1 through K0915.2 to the CDN. 
            //Only 14.
            Assert.Equal(14, Publish(new DateTime(2020, 9, 15, 13, 6, 0)).Length);
            Assert.Equal(14, w.Teks.Count(x => x.PublishingState == PublishingState.Published));

            //14.00h - POST - Server silently discards all keys as they arrive > 120 minutes after GGD code
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(new DateTime(2020, 9, 15, 14, 0, 0, DateTimeKind.Utc)), new DefaultTekValidatorConfig()).Filter(deviceTeks.ToArray(), w);
            Assert.Empty(fr.Items); 
            Assert.Equal(17, fr.Messages.Length); //14.00h Server silently discards all keys as they arrive > 120 minutes after GGD code

            deviceTeks.Add(GenerateTek(9, 15, 4));
            Assert.Equal(18, deviceTeks.Count);

            Assert.Empty(Publish(new DateTime(2020, 9, 15, 23, 59, 0)));

            //Sep 16
            deviceTeks.RemoveAt(0);
            deviceTeks.Add(GenerateTek(9, 16, 1));
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
        public void Gaen14or15SameDayTekReleaseOff()
        {
            var keysOnDevice = Enumerable.Range(1, 14).Select(x => GenerateTek(9, x, 1)).ToList();

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
            keysOnDevice.Add(GenerateTek(9, 15, 1));
            Assert.Equal(14, keysOnDevice.Count);

            //Sep 15 00:30
            T = new DateTime(2020, 9, 15, 0, 30, 0, DateTimeKind.Utc);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(new[] { lastOfSet }, w);
            Assert.Equal(1, fr.Items.Length);
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
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(keysOnDevice.Take(keysOnDevice.Count-1).ToArray(), w);
            Assert.Empty(fr.Messages);
            Write(w, fr.Items);
            Assert.Equal(keysOnDevice.Count-1, w.Teks.Count);

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
            keysOnDevice.Add(GenerateTek(9, 16, 1));
            Assert.Equal(14, keysOnDevice.Count);
            //The nightly batch uploads all keys, including now K0915.1 to bucket B ???

            T = new DateTime(2020, 9, 16, 0, 30, 0, DateTimeKind.Utc);
            fr = new BackwardCompatibleV15TekListWorkflowFilter(new FakedNow(T), new DefaultTekValidatorConfig()).Filter(keysOnDevice.ToArray(), w);
            Assert.Equal(1, fr.Items.Length);
            Assert.Equal(13, fr.Messages.Length);
            Write(w, fr.Items);

            T = new DateTime(2020, 9, 16, 2, 30, 0, DateTimeKind.Utc);
            Assert.Equal(1, Publish(T).Length);
        }
    }
}