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
    internal class FixedResultHttpGetIksCommand : IIHttpGetIksCommand
    {
        private const string DateFormatString = "yyyyMMdd";
        private readonly Dictionary<string, List<HttpGetIksSuccessResult>> _Responses;
        private readonly Dictionary<string, int> _CallIndexes = new Dictionary<string, int>();

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksSuccessResult> responses)
        {
            return Create(responses, DateTime.Now);
        }

        public static FixedResultHttpGetIksCommand Create(List<HttpGetIksSuccessResult> responses, DateTime date)
        {
            return new FixedResultHttpGetIksCommand(new Dictionary<string, List<HttpGetIksSuccessResult>>
            {
                {date.ToString(DateFormatString), responses}
            });
        }

        private FixedResultHttpGetIksCommand(Dictionary<string, List<HttpGetIksSuccessResult>> responses)
        {
            _Responses = responses;
            foreach (var key in responses.Keys) _CallIndexes[key] = 0;
        }

        public void AddItem(HttpGetIksSuccessResult item, DateTime date)
        {
            var dateString = date.ToString(DateFormatString);

            if (!_Responses.ContainsKey(dateString))
            {
                _Responses[dateString] = new List<HttpGetIksSuccessResult>();
                _CallIndexes[dateString] = 0;
            }

            var dateResponses = _Responses[dateString];

            if (dateResponses.Count > 0)
                dateResponses.Last().NextBatchTag = item.BatchTag;

            dateResponses.Add(item);
        }

        public void AddItem(HttpGetIksSuccessResult item)
        {
            AddItem(item, DateTime.Now);
        }

        public Task<HttpGetIksSuccessResult?> ExecuteAsync(string batchTag, DateTime date)
        {
            HttpGetIksSuccessResult? result = null;
            var dateString = date.ToString(DateFormatString);

            if (_CallIndexes.ContainsKey(dateString) && _CallIndexes[dateString] < _Responses[dateString].Count)
            {
                result = _Responses[dateString][_CallIndexes[dateString]++];
            }

            return Task.FromResult(result);
        }
    }
}