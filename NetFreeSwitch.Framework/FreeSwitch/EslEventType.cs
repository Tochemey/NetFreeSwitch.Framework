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

namespace NetFreeSwitch.Framework.FreeSwitch {
    public enum EslEventType {
        SESSION_CONNECT,
        SESSION_DISCONNECT,
        COMMAND_REPLY,
        API_RESPONSE,
        UN_HANDLED_EVENT,
        UN_HANDLED_MESSAGE,
        ERROR,
        AUTHENTICATION,
        DISCONNECT_NOTICE,
        CUSTOM,
        CHANNEL_CREATE,
        CHANNEL_DESTROY,
        CHANNEL_DATA,
        CHANNEL_STATE,
        CHANNEL_ANSWER,
        CHANNEL_APPLICATION,
        CHANNEL_HANGUP,
        CHANNEL_HANGUP_COMPLETE,
        CHANNEL_EXECUTE,
        CHANNEL_EXECUTE_COMPLETE,
        CHANNEL_BRIDGE,
        CHANNEL_UNBRIDGE,
        CHANNEL_PROGRESS,
        CHANNEL_PROGRESS_MEDIA,
        CHANNEL_ORIGINATE,
        CHANNEL_OUTGOING,
        CHANNEL_PARK,
        CHANNEL_UNPARK,
        CALL_UPDATE,
        CHANNEL_CALL_STATE,
        CHANNEL_UUID,
        API,
        LOG,
        INBOUND_CHAN,
        OUTBOUND_CHAN,
        STARTUP,
        SHUTDOWN,
        SESSION_HEARTBEAT,
        PUBLISH,
        UNPUBLISH,
        TALK,
        NOTALK,
        SESSION_CRASH,
        MODULE_LOAD,
        DTMF,
        MESSAGE,
        PRESENCE_IN,
        PRESENCE_OUT,
        PRESENCE_PROBE,
        SOFIA_REGISTER,
        SOFIA_EXPIRES,
        ROSTER,
        CODEC,
        BACKGROUND_JOB,
        DETECTED_SPEECH,
        PRIVATE_COMMAND,
        HEARTBEAT,
        RECORD_START,
        RECORD_STOP,
        ALL
    }
}
