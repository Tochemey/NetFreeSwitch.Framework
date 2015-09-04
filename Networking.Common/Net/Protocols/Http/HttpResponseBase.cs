﻿using System;
using System.Net;

namespace Networking.Common.Net.Protocols.Http {
    /// <summary>
    ///     A HTTP response with minimal parsing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The purpose of this class is to do as little as possible with the response to make the processing more
    ///         straightforward and without
    ///         any unnessacary steps.
    ///     </para>
    /// </remarks>
    public class HttpResponseBase : HttpMessage, IHttpResponse {
        private string _reasonPhrase;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpResponseBase" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonPhrase">The reason phrase.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <exception cref="System.ArgumentNullException">reasonPhrase</exception>
        internal HttpResponseBase(int statusCode, string reasonPhrase, string httpVersion) : base(httpVersion) {
            if (reasonPhrase == null) throw new ArgumentNullException("reasonPhrase");
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpResponseBase" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="reasonPhrase">The reason phrase.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <exception cref="System.ArgumentNullException">reasonPhrase</exception>
        public HttpResponseBase(HttpStatusCode statusCode, string reasonPhrase, string httpVersion) : base(httpVersion) {
            if (reasonPhrase == null) throw new ArgumentNullException("reasonPhrase");
            StatusCode = (int) statusCode;
            ReasonPhrase = reasonPhrase;
            Headers["Server"] = "griffinframework.net";
            Headers["Date"] = DateTime.UtcNow.ToString("R");
            Headers["Content-Type"] = "text/html";
        }

        /// <summary>
        ///     HTTP status code. You typically choose one of <see cref="System.Net.HttpStatusCode" />.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Why the specified <see cref="StatusCode" /> was set.
        /// </summary>
        public string ReasonPhrase
        {
            get { return _reasonPhrase; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _reasonPhrase = value;
            }
        }

        /// <summary>
        ///     Status line for HTTP responses is "HttpVersion StatusCode ReasonPhrase"
        /// </summary>
        public override string StatusLine { get { return HttpVersion + " " + StatusCode + " " + ReasonPhrase; } }
    }
}