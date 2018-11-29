using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.Images.Transforms
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
