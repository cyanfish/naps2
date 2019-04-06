using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Images.Transforms
{
    public class ThumbnailTransform : Transform
    {
        public ThumbnailTransform()
        {
            // TODO: Set this from the clients
            Size = 256;
            //Size = ConfigScopes.User.Current.ThumbnailSize;
        }

        public ThumbnailTransform(int size)
        {
            Size = size;
        }

        public int Size { get; }
    }
}
