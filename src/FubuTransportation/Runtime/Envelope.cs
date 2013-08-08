﻿using System;
using FubuMVC.Core.Http;

namespace FubuTransportation.Runtime
{
    [Serializable]
    public class Envelope
    {
        public static readonly string Id = "Id";
        public static readonly string OriginalId = "OriginalId";
        public static readonly string ParentId = "ParentId";
        public static readonly string ContentTypeKey = HttpResponseHeaders.ContentType;
        public static readonly string SourceKey = "Source";
        public static readonly string ChannelKey = "Channel";

        public byte[] Data;

        [NonSerialized] public object Message;

        // TODO -- do routing slip tracking later

        public Envelope(IHeaders headers)
        {
            Headers = headers;
        }

        public Envelope()
        {
            Headers = new NameValueHeaders();
        }

        public Uri Source
        {
            get { return Headers[SourceKey].ToUri(); }
            set { Headers[SourceKey] = value == null ? null : value.ToString(); }
        }

        public string ContentType
        {
            get { return Headers[ContentTypeKey]; }
            set { Headers[ContentTypeKey] = value; }
        }

        public IHeaders Headers { get; private set; }

        // TODO -- get this on the receive too!
        public Guid CorrelationId { get; set; }
    }
}