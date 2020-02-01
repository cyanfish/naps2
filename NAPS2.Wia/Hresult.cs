namespace NAPS2.Wia
{
    /// <summary>
    /// Common HRESULT codes.
    ///
    /// https://docs.microsoft.com/en-us/windows/desktop/seccrypto/common-hresult-values
    /// </summary>
    public static class Hresult
    {
        public const uint S_OK = 0x00000000;
        public const uint E_ABORT = 0x80004004;
        public const uint E_ACCESSDENIED = 0x80070005;
        public const uint E_FAIL = 0x80004005;
        public const uint E_HANDLE = 0x80070006;
        public const uint E_INVALIDARG = 0x80070057;
        public const uint E_NOINTERFACE = 0x80004002;
        public const uint E_NOTIMPL = 0x80004001;
        public const uint E_OUTOFMEMORY = 0x8007000E;
        public const uint E_POINTER = 0x80004003;
        public const uint E_UNEXPECTED = 0x8000FFFF;
    }
}