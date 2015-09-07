namespace Networking.Common.Net.Protocols.FreeSwitch {
    public enum CallState {
        DOWN = 0,
        DIALING = 1,
        RINGING = 2,
        EARLY,
        ACTIVE,
        HELD,
        HANGUP = 10
    }
}