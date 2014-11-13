/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public interface IScannedImage : IDisposable
    {
        /// <summary>
        /// Gets a thumbnail bitmap for the image. The consumer should NOT call Dispose on the returned bitmap.
        /// </summary>
        Bitmap Thumbnail { get; }

        /// <summary>
        /// Gets a copy of the scanned image. The consumer is responsible for calling Dispose on the returned bitmap.
        /// </summary>
        /// <returns>A copy of the scanned image.</returns>
        Bitmap GetImage();

        /// <summary>
        /// Transforms (rotates and/or flips) the image.
        /// </summary>
        /// <param name="rotateFlipType">The transformation type.</param>
        void RotateFlip(RotateFlipType rotateFlipType);

        /// <summary>
        /// Indicates the the scanned image has been moved to the given position in the scanned image list.
        /// </summary>
        /// <param name="index">The index at which the image was inserted after being removed.</param>
        void MovedTo(int index);
    }
}
