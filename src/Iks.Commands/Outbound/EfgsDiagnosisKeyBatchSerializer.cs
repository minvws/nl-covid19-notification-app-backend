// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Iks.Protobuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    /// <summary>
    /// TODO Write to a stream instead of an array 
    /// Serializes the DiagnosisKeyBatch in the Efgs Signing format as described here:
    /// https://github.com/eu-federation-gateway-service/efgs-federation-gateway/blob/master/docs/software-design-federation-gateway-service.md#32-signature-verification
    /// </summary>
    public class EfgsDiagnosisKeyBatchSerializer
    {
        private List<byte> _Data;

        public byte[] Serialize(DiagnosisKeyBatch batch)
        {
            var batchArray = new List<List<byte>>();

            foreach (var key in batch.Keys)
            {
                _Data = new List<byte>();

                Add(key.KeyData);
                AddSeparator();
                Add(key.RollingStartIntervalNumber);
                AddSeparator();
                Add(key.RollingPeriod);
                AddSeparator();
                Add(key.TransmissionRiskLevel);
                AddSeparator();
                Add(key.VisitedCountries);
                AddSeparator();
                Add(key.Origin);
                AddSeparator();
                Add((int)key.ReportType);
                AddSeparator();
                Add(key.DaysSinceOnsetOfSymptoms);
                AddSeparator();

                batchArray.Add(_Data);
            }

            // Sort by the base64 representation of the entire row serialized to EFGS format
            var s = batchArray.OrderBy(_ => Convert.ToBase64String(_.ToArray()), StringComparer.Ordinal);
            var resultArray = new List<byte>();
            foreach (var x in s) resultArray.AddRange(x);

            return resultArray.ToArray();
        }

        private void Add(RepeatedField<string> data)
        {
            AddBase64(Encoding.ASCII.GetBytes(string.Join(",", data)));
        }

        private void Add(string data)
        {
            AddBase64(Encoding.ASCII.GetBytes(data));
        }

        private void Add(int data)
        {
            var bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            AddBase64(bytes);
        }

        private void Add(uint data)
        {
            var bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            AddBase64(bytes);
        }
        
        private void Add(ByteString data)
        {
            AddBase64(data.ToByteArray());
        }

        private void AddSeparator()
        {
            _Data.AddRange(Encoding.ASCII.GetBytes("."));
        }
        
        private void AddBase64(byte[] data)
        {
            var base64String = Convert.ToBase64String(data);
            var bytes = Encoding.ASCII.GetBytes(base64String);

            _Data.AddRange(bytes);
        }
    }
}
