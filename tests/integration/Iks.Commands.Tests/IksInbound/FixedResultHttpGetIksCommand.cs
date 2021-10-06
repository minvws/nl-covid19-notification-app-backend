// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound
{
    /// <summary>
    /// IHttpGetIksCommand which returns the provided results in order.
    /// Use for tests.
    /// </summary>
    internal class FixedResultHttpGetIksCommand : IHttpGetIksCommand
    {
        private const string DateFormatString = "yyyyMMdd";
        private readonly Dictionary<string, List<HttpGetIksResult>> _responses;
        private readonly Dictionary<string, int> _callIndexes = new Dictionary<string, int>();

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksResult> responses)
        {
            return Create(responses, DateTime.Now);
        }

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksResult> responses, DateTime date)
        {
            return new FixedResultHttpGetIksCommand(new Dictionary<string, List<HttpGetIksResult>>
            {
                {date.ToString(DateFormatString), responses}
            });
        }

        private FixedResultHttpGetIksCommand(Dictionary<string, List<HttpGetIksResult>> responses)
        {
            _responses = responses;
            foreach (var key in responses.Keys)
            {
                _callIndexes[key] = 0;
            }
        }

        public void AddItem(HttpGetIksResult item, DateTime date)
        {
            var dateString = date.ToString(DateFormatString);

            if (!_responses.ContainsKey(dateString))
            {
                _responses[dateString] = new List<HttpGetIksResult>();
                _callIndexes[dateString] = 0;
            }

            var dateResponses = _responses[dateString];

            if (dateResponses.Count > 0)
            {
                dateResponses.Last().NextBatchTag = item.BatchTag;
            }

            dateResponses.Add(item);
        }

        public void AddItem(HttpGetIksResult item)
        {
            AddItem(item, DateTime.Now);
        }

        public Task<HttpGetIksResult> ExecuteAsync(DateTime date, string batchTag = null)
        {
            HttpGetIksResult result = null;
            var dateString = date.ToString(DateFormatString);

            //TODO: If batchTag is null, return the first batch from date; otherwise get the batch matching batchTag
            if (_callIndexes.ContainsKey(dateString) && _callIndexes[dateString] < _responses[dateString].Count)
            {
                result = _responses[dateString][_callIndexes[dateString]++];
                result.ResultCode = HttpStatusCode.OK;
            }
            else
            {
                result = new HttpGetIksResult
                {
                    BatchTag = string.Empty,
                    NextBatchTag = null,
                    ResultCode = HttpStatusCode.Gone //returned by EFGS when a batch is requested that's too old 
                };
            }

            return Task.FromResult(result);
        }
    }
}
