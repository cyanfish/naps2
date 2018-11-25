using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrParams : IEquatable<OcrParams>
    {
        public OcrParams()
        {
        }

        public OcrParams(string langCode, OcrMode mode)
        {
            LanguageCode = langCode;
            Mode = mode;
        }

        public string LanguageCode { get; set; }

        public OcrMode Mode { get; set; }

        public bool Equals(OcrParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(LanguageCode, other.LanguageCode) && Mode == other.Mode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OcrParams) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LanguageCode != null ? LanguageCode.GetHashCode() : 0) * 397) ^ (int) Mode;
            }
        }

        public static bool operator ==(OcrParams left, OcrParams right) => Equals(left, right);

        public static bool operator !=(OcrParams left, OcrParams right) => !Equals(left, right);
    }
}
