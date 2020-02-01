namespace NAPS2.Wia
{
    /// <summary>
    /// Error code constants.
    ///
    /// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-error-codes
    /// </summary>
    public static class WiaErrorCodes
    {
        public const uint PAPER_EMPTY = 0x80210003;
        public const uint NO_DEVICE_AVAILABLE = 0x80210015;
        public const uint OFFLINE = 0x80210005;
        public const uint PAPER_JAM = 0x80210002;
        public const uint BUSY = 0x80210006;
        public const uint COVER_OPEN = 0x80210016;
        public const uint COMMUNICATION = 0x8021000A;
        public const uint LOCKED = 0x8021000D;
        public const uint INCORRECT_SETTING = 0x8021000C;
        public const uint LAMP_OFF = 0x80210017;
        public const uint WARMING_UP = 0x80210007;
    }
}
