﻿using System;
using System.IO;
using System.Net;
using Networking.Common.Net.Protocols.Http.Messages;
using Networking.Common.Net.Protocols.Http.Serializers;
using Networking.Common.Net.Protocols.Serializers;

namespace Networking.Common.Net.Protocols.Http {
    /// <summary>
    ///     HTTP request, but without any operations done of the content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         the purpose with the split between this <c>Base</c> and the <c>HttpRequest</c> is that application servers,
    ///         proxies etc are not interested in the content. This wasting memory on parsing the content will hurt performance
    ///         a lot in a .NET environment.
    ///     </para>
    ///     <para>
    ///         However, every time you've configured <see cref="HttpMessageDecoder" /> to use a
    ///         <see cref="IMessageSerializer" /> like
    ///         <see cref="MultipartSerializer" /> you can safely expect the request to be of type <c>HttpRequest</c>.
    ///     </para>
    /// </remarks>
    public class HttpRequestBase : HttpMessage, IHttpRequest {
        private string _httpMethod;
        private string _pathAndQuery;
        private IParameterCollection _queryString;
        private Uri _uri;

        /// <summary>
        /// </summary>
        /// <param name="httpMethod">Method like <c>POST</c>.</param>
        /// <param name="pathAndQuery">Absolute path and query string (if one exist)</param>
        /// <param name="httpVersion">HTTP version like <c>HTTP/1.1</c></param>
        public HttpRequestBase(string httpMethod, string pathAndQuery, string httpVersion) : base(httpVersion) {
            HttpMethod = httpMethod;
            Uri = new Uri("http://localhost" + pathAndQuery);
        }

        /// <summary>
        ///     Collection of parameters extracted from the requested URI.
        /// </summary>
        public IParameterCollection QueryString
        {
            get
            {
                if (_queryString == null) {
                    _queryString = new ParameterCollection();
                    var query = Uri.Query;
                    if (query.Length > 1) // question mark is always part of the query string
                    {
                        var decoder = new UrlDecoder();
                        using (var reader = new StringReader(Uri.Query)) {
                            reader.Read(); // question mark
                            decoder.Parse(reader, _queryString);
                        }
                    }
                }

                return _queryString;
            }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _queryString = value;
            }
        }

        /// <summary>
        ///     Included cookies.
        /// </summary>
        public IHttpCookieCollection<IHttpCookie> Cookies { get; set; }

        /// <summary>
        ///     Method which was invoked.
        /// </summary>
        /// <remarks>
        ///     <para>Typically <c>GET</c>, <c>POST</c>, <c>PUT</c>, <c>DELETE</c> or <c>HEAD</c>.</para>
        /// </remarks>
        public string HttpMethod
        {
            get { return _httpMethod; }
            internal set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _httpMethod = value;
            }
        }

        /// <summary>
        ///     Request UrI
        /// </summary>
        /// <remarks>
        ///     <para>Is built using the <c>server</c> header and the path + query which is included in the request line</para>
        ///     <para>If no <c>server</c> header is included "localhost" will be used as server.</para>
        /// </remarks>
        public Uri Uri
        {
            get { return _uri; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _uri = value;
                _pathAndQuery = value.PathAndQuery;
                _queryString = null; // force regeneration on next QueryString property access
            }
        }

        /// <summary>
        ///     Address to the remote end point
        /// </summary>
        public EndPoint RemoteEndPoint { get; set; }

        IHttpResponse IHttpRequest.CreateResponse() {
            var response = new HttpResponseBase(200, "OK", HttpVersion);
            var pipeline = Headers[PipelineIndexKey];
            if (pipeline != null) response.Headers[PipelineIndexKey] = pipeline;
            return response;
        }

        /// <summary>
        ///     Status line for requests is "HttpMethod PathAndQuery HttpVersion"
        /// </summary>
        public override string StatusLine { get { return HttpMethod + " " + _pathAndQuery + " " + HttpVersion; } }

        /// <summary>
        ///     Create a response for this request.
        /// </summary>
        /// <returns>Response</returns>
        /// <remarks>
        ///     <para>
        ///         If you override this method you have to copy the PipelineIndexKey header like this:
        ///         <code>
        ///  var pipeline = Headers[PipelineIndexKey];
        ///  if (pipeline != null)
        ///  {
        ///     response.Headers[PipelineIndexKey] = pipeline;
        ///  }        
        /// </code>
        ///     </para>
        /// </remarks>
        public virtual HttpResponseBase CreateResponse() {
            var response = new HttpResponseBase(200, "OK", HttpVersion);
            var pipeline = Headers[PipelineIndexKey];
            if (pipeline != null) response.Headers[PipelineIndexKey] = pipeline;
            return response;
        }

        /// <summary>
        ///     Invoked every time a HTTP header is modified.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Value</param>
        /// <remarks>
        ///     Used to build the URI when the HOST header comes.
        /// </remarks>
        protected override void OnHeaderSet(string name, string value) {
            if (name.Equals("host", StringComparison.OrdinalIgnoreCase)) {
                //TODO: Identify schema
                Uri = new Uri("http://" + value + _pathAndQuery);
            }


            base.OnHeaderSet(name, value);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString() { return Uri.ToString(); }
    }
}