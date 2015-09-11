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

using System;
using System.Threading.Tasks;
using NetFreeSwitch.Framework.FreeSwitch.Messages;

namespace NetFreeSwitch.Framework.FreeSwitch {
    /// <summary>
    ///     Call Handler.
    /// </summary>
    public interface ICallHandler {
        /// <summary>
        ///     Answer a connected call in the outbound mode
        /// </summary>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Answer();

        /// <summary>
        ///     BindDigitAction uses the concept of realms for binding various actions.
        ///     A realm is similar to a dialplan context where the dialed number means something different depending upon which
        ///     context the call is in.
        ///     More info on https://wiki.freeswitch.org/wiki/Misc._Dialplan_Tools_bind_digit_action
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="command">The bind_digit_action command </param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> BindDigitAction(Guid id, string command, bool eventLock = false);

        /// <summary>
        ///     Bridge an existing call to other calls.
        /// </summary>
        /// <param name="id">Existing call id</param>
        /// <param name="brigdeText">bridge text definining the other calls settings</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Bridge(Guid id, string brigdeText);

        /// <summary>
        ///     Hangup a channel.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="reason">Hangup Cause <see cref="EslHangupCause" /></param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Hangup(Guid id, string reason);

        /// <summary>
        ///     Play an audio file to a given channel asynchronouly if eventLock is set to false and synchronously when it is set
        ///     to true.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="filePath">Audio file</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> PlayAudioFile(Guid id, string filePath, bool eventLock);

        /// <summary>
        ///     It establishes media (early media) but does not answer the call. You can use it to play some audio file or read
        ///     some text or wait for a while.
        /// </summary>
        /// <param name="id">Existing call id</param>
        /// <returns>Async Task</returns>
        Task<CommandReply> PreAnswer(Guid id);

        /// <summary>
        ///     Record a channel  asynchronouly if eventLock is set to false and synchronously when it is set to true.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="path">Recording location</param>
        /// <param name="timeLimit">the maximum duration of the recording in seconds.</param>
        /// <param name="silenceThreshhold">the energy level below which is considered silence.</param>
        /// <param name="silenceHits">
        ///     how many seconds of audio below silence_thresh will be tolerated before the recording stops.
        ///     When omitted, the default value is 3.
        /// </param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Record(Guid id, string path, int? timeLimit, int? silenceThreshhold, int? silenceHits, bool eventLock);

        /// <summary>
        ///     This causes an 180 Ringing to be sent to the originator.
        /// </summary>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> RingReady(bool eventLock);

        /// <summary>
        ///     The say application will use the pre-recorded sound files to read or say various things like dates, times, digits,
        ///     etc. The say application can read digits and numbers as well as dollar amounts, date/time values and IP addresses.
        ///     It can also spell out alpha-numeric text, including punctuation marks.
        ///     The execution is done asynchronouly if eventLock is set to false and synchronously when it is set to true.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="language">the language (e.g : en)</param>
        /// <param name="type">The type <see cref="EslSayTypes" /></param>
        /// <param name="method">The method <see cref="EslSayMethods" /></param>
        /// <param name="gender">The gender <see cref="EslSayGenders" /></param>
        /// <param name="text">The text to read</param>
        /// <param name="loop">The number of times to read the text</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Say(Guid id, string language, EslSayTypes type, EslSayMethods method, EslSayGenders gender, string text, int loop, bool eventLock);

        /// <summary>
        ///     Used with to BindDigitAction select the realm for which digits will be applied when digits are bound.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="realm">the digit realm, somewhat similar to a dialplan context</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> SetDigitActionRealm(Guid id, string realm, bool eventLock);

        /// <summary>
        ///     Set or Unset a channel variable asynchronously.
        ///     If the value is null it will attempt to unset the variable if it exists.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> SetVariable(Guid id, string name, string value);

        /// <summary>
        ///     Set or Unset a channel variable asynchronouly if eventLock is set to false and synchronously when it is set to
        ///     true.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> SetVariable(Guid id, string name, string value, bool eventLock);

        /// <summary>
        ///     Pause the channel for a given number of milliseconds, consuming the audio for that period of time asynchronously.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="milliSeconds">the duration</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Sleep(Guid id, int milliSeconds);

        /// <summary>
        ///     Pause the channel for a given number of milliseconds, consuming the audio for that period of time.
        ///     if eventLock is set to True the request is done synchronously.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="milliSeconds">Duration</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Sleep(Guid id, int milliSeconds, bool eventLock);

        /// <summary>
        ///     Read a text to a given channel asynchronouly if eventLock is set to false and synchronously when it is set to true.
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="engine">TTS engine</param>
        /// <param name="voice">TTS voice</param>
        /// <param name="text">The text to read</param>
        /// <param name="timerName">Timer name</param>
        /// <param name="loop">The number of times to read the text</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> Speak(Guid id, string engine, string voice, string text, string timerName, int loop, bool eventLock);

        /// <summary>
        ///     You can use StartDtmf in a dialplan to enable in-band DTMF detection (i.e. the detection of DTMF tones on a
        ///     channel). You should do this when you want to be able to identify DTMF tones on a channel that doesn't otherwise
        ///     support another signaling method (like RFC2833 or INFO).
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> StartDtmf(Guid id, bool eventLock);

        /// <summary>
        ///     Disable in-band DTMF detection  asynchronouly if eventLock is set to false and synchronously when it is set to
        ///     true.
        /// </summary>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns>Async Command Reply</returns>
        Task<CommandReply> StopDtmf(bool eventLock = false);
    }
}
