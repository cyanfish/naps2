using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NAPS2.Scan.Images.Transforms
{
    [XmlInclude(typeof(RotationTransform))]
    [XmlInclude(typeof(CropTransform))]
    [XmlInclude(typeof(BrightnessTransform))]
    [XmlInclude(typeof(ContrastTransform))]
    public abstract class Transform
    {
        public static Bitmap PerformAll(Bitmap bitmap, IEnumerable<Transform> transforms)
        {
            return transforms.Aggregate(bitmap, (current, t) => t.Perform(current));
        }

        public static void AddOrSimplify(IList<Transform> transformList, Transform transform)
        {
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
            else if (!transform.IsNull)
            {
                transformList.Add(transform);
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
        public virtual bool CanSimplify(Transform other)
        {
            return false;
        }

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
        public virtual bool IsNull { get { return false; } }
    }
}
