// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<string, List<HttpGetIksSuccessResult>> _responses;
        private readonly Dictionary<string, int> _callIndexes = new Dictionary<string, int>();

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksSuccessResult> responses)
        {

            //TODO don't map response to dates, just map responses to batchTags

            return Create(responses, DateTime.Now.Date.ToString("yyyyMMdd"));
        }

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksSuccessResult> responses, string batchTag)
        {
            return new FixedResultHttpGetIksCommand(new Dictionary<string, List<HttpGetIksSuccessResult>>
            {
                {batchTag, responses}
            });
        }

        private FixedResultHttpGetIksCommand(Dictionary<string, List<HttpGetIksSuccessResult>> responses)
        {
            _responses = responses;
            foreach (var key in responses.Keys)
            {
                _callIndexes[key] = 0;
            }
        }

        public void AddItem(HttpGetIksSuccessResult item, DateTime date)
        {
            var dateString = date.ToString(DateFormatString);

            if (!_responses.ContainsKey(dateString))
            {
                _responses[dateString] = new List<HttpGetIksSuccessResult>();
                _callIndexes[dateString] = 0;
            }

            var dateResponses = _responses[dateString];

            if (dateResponses.Count > 0)
            {
                dateResponses.Last().NextBatchTag = item.BatchTag;
            }

            dateResponses.Add(item);
        }

        public void AddItem(HttpGetIksSuccessResult item)
        {
            AddItem(item, DateTime.Now);
        }

        public Task<HttpGetIksSuccessResult> ExecuteAsync(DateTime date, string batchTag = null)
        {
            HttpGetIksSuccessResult result = null;
            var dateString = date.ToString(DateFormatString);

            //TODO: If batchTag is null, return the first batch from date; otherwise get the batch matching batchTag

            if (_callIndexes.ContainsKey(dateString) && _callIndexes[dateString] < _responses[dateString].Count)
            {
                result = _responses[dateString][_callIndexes[dateString]++];
            }

            return Task.FromResult(result);
        }
    }
}
