﻿using System;
using FubuMVC.Core.Http;
using FubuCore;

namespace FubuTransportation.Runtime
{
    // TODO -- give this a decent ToString()
    [Serializable]
    public class Envelope
    {
        public static readonly string OriginalIdKey = "OriginalId";
        public static readonly string IdKey = "Id";
        public static readonly string ParentIdKey = "ParentId";
        public static readonly string ContentTypeKey = HttpResponseHeaders.ContentType;
        public static readonly string SourceKey = "Source";
        public static readonly string ChannelKey = "Channel";
        public static readonly string ReplyRequestedKey = "Reply-Requested";
        public static readonly string ResponseIdKey = "Response";
        public static readonly string DestinationKey = "Destination";

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
            CorrelationId = Guid.NewGuid().ToString();
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

        public string OriginalId
        {
            get { return Headers[OriginalIdKey]; }
            set { Headers[OriginalIdKey] = value; }
        }

        public string ParentId
        {
            get { return Headers[ParentIdKey]; }
            set { Headers[ParentIdKey] = value; }
        }

        public string ResponseId
        {
            get { return Headers[ResponseIdKey]; }
            set { Headers[ResponseIdKey] = value; }
        }

        public Uri Destination
        {
            get { return Headers[DestinationKey].ToUri(); }
            set { Headers[DestinationKey] = value == null ? null : value.ToString(); }
        }

        public IHeaders Headers { get; private set; }

        public string CorrelationId
        {
            get
            {
                return Headers[IdKey];
            }
            set { Headers[IdKey] = value; }
        }

        public bool ReplyRequested
        {
            get { return Headers.Has(ReplyRequestedKey) ? Headers[ReplyRequestedKey].EqualsIgnoreCase("true") : false; }
            set
            {
                if (value)
                {
                    Headers[ReplyRequestedKey] = "true";
                }
                else
                {
                    Headers.Remove(ReplyRequestedKey);
                }
            }
        }

        // TODO -- this is where the routing slip is going to come into place
        public Envelope ForResponse(object message)
        {
            var child = new Envelope
            {
                Message = message,
                OriginalId = OriginalId ?? CorrelationId,
                ParentId = CorrelationId
            };

            if (Headers.Has(ReplyRequestedKey) && Headers[ReplyRequestedKey].EqualsIgnoreCase("true"))
            {
                child.Headers[ResponseIdKey] = CorrelationId;
                child.Destination = Source;
            }

            return child;
        }
    }
}