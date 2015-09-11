/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

namespace NetFreeSwitch.Framework.Net.Channels {
    /// <summary>
    ///     Delegate for <see cref="ITcpChannel.BufferPreProcessor" />.
    /// </summary>
    /// <param name="channel">Channel that have received something</param>
    /// <param name="buffer">Buffer containing the processed bytes</param>
    /// <returns>Number of bytes process by the buffer pre processor (i.e. no one else should process those bytes)</returns>
    public delegate int BufferPreProcessorHandler(ITcpChannel channel, ISocketBuffer buffer);
}
