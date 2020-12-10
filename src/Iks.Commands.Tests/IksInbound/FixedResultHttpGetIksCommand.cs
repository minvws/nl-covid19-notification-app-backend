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
    class FixedResultHttpGetIksCommand : IIHttpGetIksCommand
    {
        private readonly List<HttpGetIksSuccessResult> _Responses;
        private int _CallIndex;

        public FixedResultHttpGetIksCommand(List<HttpGetIksSuccessResult> responses)
        {
            _Responses = responses;
        }
        
        public void AddItem(HttpGetIksSuccessResult item)
        {
            if (_Responses.Count > 0) _Responses.Last().NextBatchTag = item.BatchTag;
            _Responses.Add(item);
        }

        public Task<HttpGetIksSuccessResult?> ExecuteAsync(string batchTag, DateTime date)
        {
            HttpGetIksSuccessResult? result = null;

            if(_CallIndex < _Responses.Count)
            {
                result = _Responses[_CallIndex++];
            }

            return Task.FromResult(result);
        }
    }
}