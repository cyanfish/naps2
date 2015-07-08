using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NAPS2.Scan.Images.Transforms
{
    public class TransformSet
    {
        public Bitmap PerformAll(Bitmap bitmap)
        {
            return AllTransforms.Aggregate(bitmap, (current, t) => t.Perform(current));
        }

        public IEnumerable<Transform> AllTransforms
        {
            get
            {
                if (Rotation != null)
                {
                    yield return Rotation;
                }
            }
        }

        public RotationTransform Rotation { get; set; }
    }
}
