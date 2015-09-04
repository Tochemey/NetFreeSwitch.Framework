﻿namespace Networking.Common.Net.Protocols.FreeSwitch
{
    public enum EslEventList
    {
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