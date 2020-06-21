using System;
using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    [ProtoContract]
    public class ContentArgs
    {
        [ProtoMember(1)]
        public string PublishingId { get; set; }

        [ProtoMember(2)]
        public DateTime Released { get; set; }

        [ProtoMember(3)]
        //Zipped, signed content
        public byte[] Content { get; set; }
    }
}