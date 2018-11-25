using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.Scan.Images.Transforms
{
    [XmlInclude(typeof(RotationTransform))]
    [XmlInclude(typeof(CropTransform))]
    [XmlInclude(typeof(BrightnessTransform))]
    [XmlInclude(typeof(ContrastTransform))]
    [XmlInclude(typeof(TrueContrastTransform))]
    [XmlInclude(typeof(SharpenTransform))]
    [XmlInclude(typeof(HueTransform))]
    [XmlInclude(typeof(SaturationTransform))]
    [XmlInclude(typeof(BlackWhiteTransform))]
    [Serializable]
    public abstract class Transform
    {
        public static Bitmap PerformAll(Bitmap bitmap, IEnumerable<Transform> transforms)
        {
            return transforms.Aggregate(bitmap, (current, t) => t.Perform(current));
        }

        public static bool AddOrSimplify(IList<Transform> transformList, Transform transform)
        {
            if (transform.IsNull)
            {
                return false;
            }
            var last = transformList.LastOrDefault();
            if (transform.CanSimplify(last))
            {
                var simplified = transform.Simplify(last);
                if (simplified.IsNull)
                {
                    transformList.RemoveAt(transformList.Count - 1);
                }
                else
                {
                    transformList[transformList.Count - 1] = transform.Simplify(last);
                }
            }
            else
            {
                transformList.Add(transform);
            }
            return true;
        }

        /// <summary>
        /// If the provided bitmap is 1-bit (black and white), replace it with a 24-bit bitmap so that image transforms will work. If the bitmap is replaced, the original is disposed.
        /// </summary>
        /// <param name="bitmap">The bitmap that may be replaced.</param>
        protected static void EnsurePixelFormat(ref Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                // Copy B&W over to grayscale
                var bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                bitmap2.SafeSetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                using (var g = Graphics.FromImage(bitmap2))
                {
                    g.DrawImage(bitmap, 0, 0);
                }
                bitmap.Dispose();
                bitmap = bitmap2;
            }
        }

        /// <summary>
        /// If the original bitmap is 1-bit (black and white), optimize the result by making it 1-bit too.
        /// </summary>
        /// <param name="original">The original bitmap that is used to determine whether the result should be black and white.</param>
        /// <param name="result">The result that may be replaced.</param>
        protected static void OptimizePixelFormat(Bitmap original, ref Bitmap result)
        {
            if (original.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                var bitmap2 = (Bitmap)BitmapHelper.CopyToBpp(result, 1).Clone();
                result.Dispose();
                result = bitmap2;
            }
        }

        /// <summary>
        /// Returns a bitmap with the result of the transform.
        /// May be the same bitmap object if the transform can be performed in-place.
        /// The original bitmap is disposed otherwise.
        /// </summary>
        /// <param name="bitmap">The bitmap to transform.</param>
        /// <returns></returns>
        public abstract Bitmap Perform(Bitmap bitmap);

        /// <summary>
        /// Determines if this transform performed after another transform can be combined to form a single transform.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool CanSimplify(Transform other) => false;

        /// <summary>
        /// Combines this transform with a previous transform to form a single new transform.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual Transform Simplify(Transform other)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets a value that indicates whether the transform is a null transformation (i.e. has no effect).
        /// </summary>
        public virtual bool IsNull => false;
    }
}
