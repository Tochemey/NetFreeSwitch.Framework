﻿using System;
using System.IO;
using System.Runtime.Serialization;
using Networking.Common.Net.Protocols.Http.Messages;

namespace Networking.Common.Net.Protocols.Serializers {
    /// <summary>
    ///     Uses the DataContract XML serializer.
    /// </summary>
    public class DataContractMessageSerializer : IMessageSerializer {
        /// <summary>
        ///     <c>application/x-datacontract</c>
        /// </summary>
        public const string MimeType = "application/x-datacontract";

        /// <summary>
        ///     Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">
        ///     If you include the type name to it after the format name, for instance
        ///     <c>json;type=YourApp.DTO.User-YourApp</c>
        /// </param>
        public void Serialize(object source, Stream destination, out string contentType) {
            var serializer = new DataContractSerializer(source.GetType());
            serializer.WriteObject(destination, source);
            contentType = string.Format("{0};type={1}", MimeType, source.GetType().GetSimpleAssemblyQualifiedName().Replace(',', '-'));
        }

        /// <summary>
        ///     Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes { get; private set; }

        /// <summary>
        ///     Deserialize the content from the stream.
        /// </summary>
        /// <param name="contentType">
        ///     Used to identify the object which is about to be deserialized. Specified by the
        ///     <c>Serialize()</c> method when invoked in the other end point.
        /// </param>
        /// <param name="source">Stream that contains the object to deserialize.</param>
        /// <returns>
        ///     Created object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">contentType</exception>
        /// <exception cref="System.FormatException">Failed to build a type from ' + contentType + '.</exception>
        public object Deserialize(string contentType, Stream source) {
            if (contentType == null) throw new ArgumentNullException("contentType");
            var header = new HttpHeaderValue(contentType);

            // to be backwards compatible.
            Type type;
            var typeArgument = header.Parameters["type"];
            if (typeArgument == null) {
                var pos = contentType.IndexOf(';');
                if (pos == -1) type = Type.GetType(contentType, false, true);
                else type = Type.GetType(contentType.Substring(pos + 1), false, true);
            }
            else {
                var typeName = header.Parameters["type"].Replace("-", ",");
                type = Type.GetType(typeName, true);
            }

            if (type == null)
                throw new FormatException("Failed to build a type from '" + contentType + "'.");


            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(source);
        }
    }
}