using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.Ocr
{
    public class OcrRequestParams : IEquatable<OcrRequestParams>
    {
        public OcrRequestParams(ScannedImage.Snapshot snapshot, IOcrEngine ocrEngine, OcrParams ocrParams)
        {
            ScannedImage = snapshot.Source;
            TransformState = snapshot.TransformList.Count == 0 ? -1 : snapshot.TransformState;
            Engine = ocrEngine;
            OcrParams = ocrParams;
        }

        public ScannedImage ScannedImage { get; }

        public int TransformState { get; }

        public IOcrEngine Engine { get; }

        public OcrParams OcrParams { get; }

        public bool Equals(OcrRequestParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ScannedImage, other.ScannedImage) && TransformState == other.TransformState && Equals(Engine, other.Engine) && Equals(OcrParams, other.OcrParams);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OcrRequestParams) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ScannedImage != null ? ScannedImage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TransformState;
                hashCode = (hashCode * 397) ^ (Engine != null ? Engine.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OcrParams != null ? OcrParams.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(OcrRequestParams left, OcrRequestParams right) => Equals(left, right);

        public static bool operator !=(OcrRequestParams left, OcrRequestParams right) => !Equals(left, right);
    }
}