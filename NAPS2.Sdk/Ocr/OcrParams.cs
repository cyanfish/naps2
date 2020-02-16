using System;

namespace NAPS2.Ocr
{
    public class OcrParams : IEquatable<OcrParams>
    {
        public OcrParams()
        {
        }

        public OcrParams(string? langCode, OcrMode mode, double timeoutInSeconds)
        {
            LanguageCode = langCode;
            Mode = mode;
            TimeoutInSeconds = timeoutInSeconds;
        }

        public string? LanguageCode { get; set; }

        public OcrMode Mode { get; set; }

        public double TimeoutInSeconds { get; set; }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OcrParams) obj);
        }

        public bool Equals(OcrParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(LanguageCode, other.LanguageCode) && Mode == other.Mode && TimeoutInSeconds.Equals(other.TimeoutInSeconds);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LanguageCode != null ? LanguageCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Mode;
                hashCode = (hashCode * 397) ^ TimeoutInSeconds.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(OcrParams left, OcrParams right) => Equals(left, right);

        public static bool operator !=(OcrParams left, OcrParams right) => !Equals(left, right);
    }
}
