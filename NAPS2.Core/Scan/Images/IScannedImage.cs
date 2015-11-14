/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.IO;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images
{
    public interface IScannedImage : IDisposable
    {
        /// <summary>
        /// Gets a copy of the current thumbnail bitmap for the image.
        /// </summary>
        Bitmap GetThumbnail(int preferredSize);

        /// <summary>
        /// Gets an object that changes when the thumbnail is changed.
        /// </summary>
        object GetThumbnailState();

        /// <summary>
        /// Sets the current thumbnail bitmap for the image.
        /// </summary>
        void SetThumbnail(Bitmap bitmap);

        /// <summary>
        /// Renders a bitmap for the image's thumbnail with all of the transforms at the given size.
        /// </summary>
        Bitmap RenderThumbnail(int size);

        /// <summary>
        /// Gets a copy of the scanned image. The consumer is responsible for calling Dispose on the returned bitmap.
        /// </summary>
        /// <returns>A copy of the scanned image.</returns>
        Bitmap GetImage();

        /// <summary>
        /// Gets a stream for the scanned image. The consumer is responsible for calling Dispose on the returned stream.
        /// </summary>
        /// <returns>A stream for the scanned image.</returns>
        Stream GetImageStream();

        /// <summary>
        /// Adds a transform to the image.
        /// </summary>
        /// <param name="transform">The transform.</param>
        void AddTransform(Transform transform);

        /// <summary>
        /// Removes all of the transforms from the image.
        /// </summary>
        void ResetTransforms();

        /// <summary>
        /// Indicates the the scanned image has been moved to the given position in the scanned image list.
        /// </summary>
        /// <param name="index">The index at which the image was inserted after being removed.</param>
        void MovedTo(int index);

        /// <summary>
        /// Gets or sets the patch code associated with the scanned page.
        /// </summary>
        PatchCode PatchCode { get; set; }
    }
}
