using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;

namespace Microscopic
{
    public class Request
    {
        public Request(HttpListenerRequest source)
        {
            AcceptTypes = source.AcceptTypes;
            Cookies = source.Cookies;
            HasBody = source.HasEntityBody;
            Headers = source.Headers;
            IsLocal = source.IsLocal;
            IsSecure = source.IsSecureConnection;
            KeepAlive = source.KeepAlive;
            Method = new HttpMethod(source.HttpMethod);
            QueryString = source.QueryString;
            RawUrl = source.RawUrl;
            Referrer = source.UrlReferrer;
            RequestId = source.RequestTraceIdentifier.ToString();
            Url = source.Url;
            UserAgent = source.UserAgent;
            UserHostAddress = source.UserHostAddress;
            UserHostName = source.UserHostName;
        }

        public IEnumerable<string> AcceptTypes { get; }

        public CookieCollection Cookies { get; }

        public bool HasBody { get; }

        public NameValueCollection Headers { get; }

        public bool IsSecure { get; }

        public bool IsLocal { get; }

        public bool KeepAlive { get; }

        public HttpMethod Method { get; }

        public NameValueCollection QueryString { get; }

        public string RawUrl { get; }

        public Uri Referrer { get; }

        public string RequestId { get; }

        public Uri Url { get; }

        public string UserAgent { get; }

        public string UserHostAddress { get; }

        public string UserHostName { get; }
    }
}