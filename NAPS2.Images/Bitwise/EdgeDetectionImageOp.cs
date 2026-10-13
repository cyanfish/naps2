namespace NAPS2.Images.Bitwise;

/// <summary>
/// Detects the bounds of a document within a scan whose scan area is larger than the document itself (i.e. the image
/// has a border of scanner background around the real page). The results are the number of pixels to crop from each
/// side, with the same semantics as CropTransform.
/// </summary>
internal class EdgeDetectionImageOp : UnaryBitwiseImageOp
{
    // Each side of the image is split into strips, so we can find documents that don't span the whole scan (e.g. a
    // receipt in a corner) and tolerate slight skew. For each strip, the background is the median luma of the outermost
    // lines, and we scan inward until we find one of:
    //
    // - An edge: a straight line along which the luma consistently changes. For white paper on a white background this
    // is often a shadow or highlight too faint to see pixel-by-pixel through the noise. So we sum the signed gradient
    // along the line, clamping each pixel's gradient to a small value; noise cancels out while a real edge adds up.
    // A page on a dark background always contrasts with it, so there we only look for edges followed by content.
    // - Content: pixels that differ a lot from the background, i.e. text or a printed line. This means the
    // "background" is actually part of the page.
    //
    // Then the strips vote. If at least two strips agree on an edge (and fewer strips found content elsewhere), we crop
    // there. Otherwise the page extends past the side of the image and we don't crop.
    //
    // If the image has been deskewed, the white fill in its corners isn't part of the scan, so it's ignored. Similarly,
    // if the scanner padded the image with a single color (as the scan area was smaller than the image), that's cropped
    // and the rest of the image is treated as the whole scan.

    // Ignore a bit of the outside of the image so that artifacts from the edge of the scan area aren't counted.
    private const double IGNORE_EDGE_FRACTION = 0.005;
    // The fraction of each side used to estimate the background color.
    private const double BG_BAND_FRACTION = 0.01;
    // The number of strips each side is split into.
    private const int STRIPS = 8;
    // Luma (0-255) difference from the background for a pixel to count as content.
    private const int CONTENT_THRESHOLD = 40;
    // Background luma (0-255) below which pages are expected to contrast with the background.
    private const int DARK_BACKGROUND = 128;
    // The fraction of content pixels for a line (in a strip) to stop the scan. This is high enough to ignore dust.
    private const double MIN_CONTENT_FRACTION = 0.05;
    // The fraction of content pixels for a line to count as having any ink.
    private const double MIN_INK_FRACTION = 0.01;
    // A page that contrasts with the background (e.g. a photo) is mostly content pixels for some distance.
    private const double MIN_SOLID_FRACTION = 0.5;
    private const double SUSTAINED_CONTENT_FRACTION = 0.02;
    // The luma gradient at a pixel is the difference between the pixels this distance away on either side. Each
    // pixel's gradient is clamped to a small magnitude when looking for edges.
    private const int EDGE_GRADIENT_SPAN = 2;
    private const int EDGE_GRADIENT_CLAMP = 4;
    // The minimum fraction of a line (in a strip) that needs a consistent gradient for it to count as an edge.
    private const double MIN_EDGE_COVERAGE = 0.45;
    // The number of lines a sharp edge can span, given the gradient span.
    private const int EDGE_WIDTH = 2 * EDGE_GRADIENT_SPAN + 1;
    // How close (as a fraction of the dimension) edges in different strips need to be to count as the same edge.
    private const double EDGE_AGREEMENT_FRACTION = 0.01;
    private const int EDGE_AGREEMENT_MIN = 3;
    // If the detected page is smaller than this fraction of the image in either dimension, we don't crop.
    private const double MIN_PAGE_FRACTION = 0.1;
    // Deskewing fills the corners of the image with white, which shouldn't count as part of the image. The fill along
    // each side is a triangle; its depth may differ from a straight line by this many pixels.
    private const int FILL = 255;
    private const int FILL_TOLERANCE = 2;
    // Padding is a single color, give or take this much luma (e.g. from rounding in JPEG compression).
    private const int PADDING_NOISE = 2;
    // Scanned data bleeds into the padding for a few lines (from blur, or JPEG compression, which works in blocks of up
    // to 16 pixels). Those lines are mostly within a larger tolerance of the padding color.
    private const int MAX_PADDING_BLEED = 16;
    private const int PADDING_BLEED_TOLERANCE = 10;
    private const double MIN_PADDING_BLEED_FRACTION = 0.95;
    // Padding is next to scanned data, where hardly any pixels are the padding color. Otherwise it could be e.g. a
    // page where the paper is pure white (like a black & white image), with a line of text next to its margin.
    private const double MAX_FILL_NEXT_TO_PADDING = 0.1;

    // The size of the scanned area, i.e. the image without padding
    private int _w;
    private int _h;
    private bool _invertY;
    // The depth of padding (full lines of a single color) on each side
    private int _padLeft, _padTop, _padRight, _padBottom;
    // The depth of deskew fill from each side, indexed by y (for left/right) or x (for top/bottom)
    private int[]? _fillLeft, _fillRight, _fillTop, _fillBottom;
    // Background luma for each strip on each side
    private int[] _bgLeft = null!, _bgRight = null!, _bgTop = null!, _bgBottom = null!;
    // Content pixel counts for each line in each strip, indexed by [strip * w + x] or [strip * h + y]
    private int[] _contentLeft = null!, _contentRight = null!, _contentTop = null!, _contentBottom = null!;
    // Clamped gradient sums for each line in each strip, indexed by [strip * w + x] or [strip * h + y]
    private int[] _colGradients = null!, _rowGradients = null!;

    protected override LockMode LockMode => LockMode.ReadOnly;

    protected override int GetPartitionSize(BitwiseImageData data) => _h;

    /// <summary>
    /// The number of pixels to crop from the left side.
    /// </summary>
    public int Left { get; private set; }

    /// <summary>
    /// The number of pixels to crop from the top side.
    /// </summary>
    public int Top { get; private set; }

    /// <summary>
    /// The number of pixels to crop from the right side.
    /// </summary>
    public int Right { get; private set; }

    /// <summary>
    /// The number of pixels to crop from the bottom side.
    /// </summary>
    public int Bottom { get; private set; }

    protected override void ValidateCore(BitwiseImageData data)
    {
    }

    protected override void StartCore(BitwiseImageData data)
    {
        _invertY = data.invertY;
        FindPadding(data);
        _w = data.w - _padLeft - _padRight;
        _h = data.h - _padTop - _padBottom;
        _contentLeft = new int[STRIPS * _w];
        _contentRight = new int[STRIPS * _w];
        _contentTop = new int[STRIPS * _h];
        _contentBottom = new int[STRIPS * _h];
        _colGradients = new int[STRIPS * _w];
        _rowGradients = new int[STRIPS * _h];
        FindDeskewFill(data);
        EstimateBackground(data);
    }

    private void EstimateBackground(BitwiseImageData data)
    {
        int bandW = Math.Max((int) Math.Round(_w * BG_BAND_FRACTION), 2);
        int bandH = Math.Max((int) Math.Round(_h * BG_BAND_FRACTION), 2);
        var left = new long[STRIPS, 256];
        var right = new long[STRIPS, 256];
        var top = new long[STRIPS, 256];
        var bottom = new long[STRIPS, 256];
        var luma = new int[_w];
        for (int y = 0; y < _h; y++)
        {
            // Only read the pixels in the bands
            if (y < bandH || y >= _h - bandH)
            {
                ReadDataRow(data, y, luma, 0, _w);
            }
            else
            {
                ReadDataRow(data, y, luma, 0, bandW);
                ReadDataRow(data, y, luma, _w - bandW, _w);
            }
            int sy = y * STRIPS / _h;
            for (int x = 0; x < bandW; x++)
            {
                if (luma[x] >= 0) left[sy, luma[x]]++;
                if (luma[_w - 1 - x] >= 0) right[sy, luma[_w - 1 - x]]++;
            }
            if (y < bandH || y >= _h - bandH)
            {
                var hist = y < bandH ? top : bottom;
                for (int x = 0; x < _w; x++)
                {
                    if (luma[x] >= 0) hist[x * STRIPS / _w, luma[x]]++;
                }
            }
        }
        _bgLeft = Medians(left);
        _bgRight = Medians(right);
        _bgTop = Medians(top);
        _bgBottom = Medians(bottom);
    }

    private static int[] Medians(long[,] histograms)
    {
        var medians = new int[STRIPS];
        for (int s = 0; s < STRIPS; s++)
        {
            medians[s] = -1;
            long total = 0;
            for (int i = 0; i < 256; i++) total += histograms[s, i];
            long cumulative = 0;
            for (int i = 0; i < 256 && total > 0; i++)
            {
                cumulative += histograms[s, i];
                if (cumulative * 2 >= total)
                {
                    medians[s] = i;
                    break;
                }
            }
        }
        // Strips that are entirely deskew fill use the typical background of the other strips
        var known = medians.Where(x => x != -1).OrderBy(x => x).ToList();
        for (int s = 0; s < STRIPS; s++)
        {
            if (medians[s] == -1) medians[s] = known.Count > 0 ? known[known.Count / 2] : FILL;
        }
        return medians;
    }

    /// <summary>
    /// Finds full lines of a single color along each side, which some scanners use to pad the image when the scan area
    /// is smaller than the requested size. It only counts as padding if it's next to scanned data (rather than e.g. a
    /// saturated white page), and is deeper than the ignored area at the edge of the image (to keep edge artifacts as
    /// they are).
    /// </summary>
    private void FindPadding(BitwiseImageData data)
    {
        int w = data.w, h = data.h;
        int ignoreX = Math.Max((int) Math.Round(w * IGNORE_EDGE_FRACTION), 1);
        int ignoreY = Math.Max((int) Math.Round(h * IGNORE_EDGE_FRACTION), 1);
        // The lines from each side, indexed by depth, are read into a buffer (which is reused for the next line)
        var row = new int[w];
        var col = new int[h];
        int[] TopLine(int i) => ReadRow(i);
        int[] BottomLine(int i) => ReadRow(h - 1 - i);
        int[] LeftLine(int i) => ReadCol(i);
        int[] RightLine(int i) => ReadCol(w - 1 - i);
        int[] ReadRow(int y)
        {
            ReadLumaRow(data, y, row, 0, w);
            return row;
        }
        int[] ReadCol(int x)
        {
            for (int y = 0; y < h; y++)
            {
                ReadLumaRow(data, y, row, x, x + 1);
                col[y] = row[x];
            }
            return col;
        }

        // The fraction of a line (from start to end along it) within the tolerance of the padding color
        double FillFraction(int[] line, int start, int end, int fill, int tolerance)
        {
            int count = 0;
            for (int t = start; t < end; t++)
            {
                if (Math.Abs(line[t] - fill) <= tolerance) count++;
            }
            return count / (double) (end - start);
        }

        // The depth of padding along one side, with n lines of the given length, excluding the ignored area. We stop
        // looking once the padding would leave too little of the image, and return -1 (e.g. for a blank page).
        (int depth, int fill) Depth(Func<int, int[]> lines, int n, int ignore, int length, int lengthIgnore)
        {
            int maxDepth = n - (int) Math.Ceiling(n * MIN_PAGE_FRACTION);
            int fill = lines(ignore)[length / 2];
            double Fraction(int i, int tolerance) =>
                FillFraction(lines(i), lengthIgnore, length - lengthIgnore, fill, tolerance);
            int depth = ignore;
            while (depth < maxDepth && Fraction(depth, PADDING_NOISE) == 1) depth++;
            if (depth == ignore) return (0, fill);
            int bleedEnd = Math.Min(depth + MAX_PADDING_BLEED, maxDepth);
            while (depth < bleedEnd && Fraction(depth, PADDING_BLEED_TOLERANCE) >= MIN_PADDING_BLEED_FRACTION) depth++;
            return (depth < maxDepth ? depth : -1, fill);
        }
        _padLeft = _padTop = _padRight = _padBottom = 0;
        var (top, topFill) = Depth(TopLine, h, ignoreY, w, ignoreX);
        // The image is (almost) all one color, so there's no padding
        if (top == -1) return;
        var (bottom, bottomFill) = Depth(BottomLine, h, ignoreY, w, ignoreX);
        var (left, leftFill) = Depth(LeftLine, w, ignoreX, h, ignoreY);
        var (right, rightFill) = Depth(RightLine, w, ignoreX, h, ignoreY);

        // Check for scanned data past any blur, excluding the padding on the adjacent sides
        bool NextToScan(Func<int, int[]> lines, int depth, int n, int start, int end, int fill) =>
            depth > 0 && depth + EDGE_GRADIENT_SPAN < n && start < end &&
            FillFraction(lines(depth + EDGE_GRADIENT_SPAN), start, end, fill, PADDING_NOISE) <= MAX_FILL_NEXT_TO_PADDING;
        int x1 = Math.Max(left, ignoreX), x2 = w - Math.Max(right, ignoreX);
        int y1 = Math.Max(top, ignoreY), y2 = h - Math.Max(bottom, ignoreY);
        _padTop = NextToScan(TopLine, top, h, x1, x2, topFill) ? top : 0;
        _padBottom = NextToScan(BottomLine, bottom, h, x1, x2, bottomFill) ? bottom : 0;
        _padLeft = NextToScan(LeftLine, left, w, y1, y2, leftFill) ? left : 0;
        _padRight = NextToScan(RightLine, right, w, y1, y2, rightFill) ? right : 0;

        // If there's barely anything left (e.g. the image is split into two colors), it's not padding
        if (w - _padLeft - _padRight < w * MIN_PAGE_FRACTION || h - _padTop - _padBottom < h * MIN_PAGE_FRACTION)
        {
            _padLeft = _padTop = _padRight = _padBottom = 0;
        }
    }

    private void FindDeskewFill(BitwiseImageData data)
    {
        int w = _w, h = _h;
        var luma = new int[w];
        int Luma(int x, int y)
        {
            ReadScanRow(data, y, luma, x, x + 1);
            return luma[x];
        }
        // The depth of white from each side
        var left = new int[h];
        var right = new int[h];
        var top = new int[w];
        var bottom = new int[w];
        for (int y = 0; y < h; y++)
        {
            while (left[y] < w && Luma(left[y], y) == FILL) left[y]++;
            while (right[y] < w && Luma(w - 1 - right[y], y) == FILL) right[y]++;
        }
        for (int x = 0; x < w; x++)
        {
            while (top[x] < h && Luma(x, top[x]) == FILL) top[x]++;
            while (bottom[x] < h && Luma(x, h - 1 - bottom[x]) == FILL) bottom[x]++;
        }
        // Deskewing leaves a triangle of fill along every side
        if (IsTriangle(left) && IsTriangle(right) && IsTriangle(top) && IsTriangle(bottom))
        {
            (_fillLeft, _fillRight, _fillTop, _fillBottom) = (left, right, top, bottom);
        }
    }

    /// <summary>
    /// Checks whether the depth along a side goes in a straight line from one end down to zero (deskewing rotates the
    /// image around its center, so this is about halfway along the side), and stays at zero. The ends are skipped, as
    /// the fill from the adjacent sides overlaps there.
    /// </summary>
    private static bool IsTriangle(int[] depth)
    {
        int t1 = depth.Length / 10, t2 = depth.Length - 1 - depth.Length / 10;
        // Check from the deeper end
        if (depth[t1] < depth[t2]) depth = depth.Reverse().ToArray();
        int zero = Array.FindIndex(depth, t1, t2 - t1, x => x == 0);
        if (depth[t1] <= FILL_TOLERANCE || zero == -1) return false;
        double slope = depth[t1] / (double) (zero - t1);
        for (int t = t1; t <= t2; t++)
        {
            if (Math.Abs(depth[t] - Math.Max(depth[t1] - slope * (t - t1), 0)) > FILL_TOLERANCE) return false;
        }
        return true;
    }

    /// <summary>
    /// Reads the luma for part of a row, with deskew fill pixels set to -1.
    /// </summary>
    private void ReadDataRow(BitwiseImageData data, int y, int[] luma, int start, int end)
    {
        ReadScanRow(data, y, luma, start, end);
        if (_fillLeft == null) return;
        for (int x = start; x < end; x++)
        {
            if (x < _fillLeft[y] || x >= _w - _fillRight![y] || y < _fillTop![x] || y >= _h - _fillBottom![x])
            {
                luma[x] = -1;
            }
        }
    }

    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (!(data.bytesPerPixel is 1 or 3 or 4 || data.bitsPerPixel == 1))
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }

        int w = _w, h = _h;
        // Content in the ignored area around the outside of the image (e.g. scanner artifacts) isn't counted
        int ignoreX = Math.Max((int) Math.Round(w * IGNORE_EDGE_FRACTION), 1);
        int ignoreY = Math.Max((int) Math.Round(h * IGNORE_EDGE_FRACTION), 1);
        // Column-indexed values need to be summed across partitions, while row-indexed values are written directly
        // (each partition has a distinct range of rows)
        var contentLeft = new int[STRIPS * w];
        var contentRight = new int[STRIPS * w];
        var colGradients = new int[STRIPS * w];
        var stripX = new int[w];
        for (int x = 0; x < w; x++)
        {
            stripX[x] = x * STRIPS / w;
        }
        const int d = EDGE_GRADIENT_SPAN;
        // A window of rows from y - d to y + d
        var rows = new int[2 * d + 1][];
        for (int k = 0; k < rows.Length; k++)
        {
            rows[k] = new int[w];
            int ry = partStart - d + k;
            if (ry >= 0 && ry < h && k < rows.Length - 1) ReadDataRow(data, ry, rows[k], 0, w);
        }

        for (int y = partStart; y < partEnd; y++)
        {
            if (y + d < h) ReadDataRow(data, y + d, rows[rows.Length - 1], 0, w);
            var prev = rows[0];
            var cur = rows[d];
            var next = rows[rows.Length - 1];
            bool hasPrev = y - d >= 0;
            bool hasNext = y + d < h;
            int sy = y * STRIPS / h;
            bool countContentY = y >= ignoreY && y < h - ignoreY;
            int bgLeft = _bgLeft[sy], bgRight = _bgRight[sy];
            int colOffset = sy * w;

            for (int x = 0; x < w; x++)
            {
                int v = cur[x];
                int sx = stripX[x];
                int rowIndex = sx * h + y;
                if (countContentY && x >= ignoreX && x < w - ignoreX && v >= 0)
                {
                    if (Math.Abs(v - bgLeft) > CONTENT_THRESHOLD) contentLeft[colOffset + x]++;
                    if (Math.Abs(v - bgRight) > CONTENT_THRESHOLD) contentRight[colOffset + x]++;
                    if (Math.Abs(v - _bgTop[sx]) > CONTENT_THRESHOLD) _contentTop[rowIndex]++;
                    if (Math.Abs(v - _bgBottom[sx]) > CONTENT_THRESHOLD) _contentBottom[rowIndex]++;
                }

                // Deskew fill pixels are -1, and don't count towards gradients
                int gx = x >= d && x < w - d && cur[x + d] >= 0 && cur[x - d] >= 0 ? cur[x + d] - cur[x - d] : 0;
                int gy = hasPrev && hasNext && next[x] >= 0 && prev[x] >= 0 ? next[x] - prev[x] : 0;
                colGradients[colOffset + x] += Math.Max(Math.Min(gx, EDGE_GRADIENT_CLAMP), -EDGE_GRADIENT_CLAMP);
                _rowGradients[rowIndex] += Math.Max(Math.Min(gy, EDGE_GRADIENT_CLAMP), -EDGE_GRADIENT_CLAMP);
            }

            var first = rows[0];
            Array.Copy(rows, 1, rows, 0, rows.Length - 1);
            rows[rows.Length - 1] = first;
        }

        lock (this)
        {
            for (int i = 0; i < colGradients.Length; i++)
            {
                _contentLeft[i] += contentLeft[i];
                _contentRight[i] += contentRight[i];
                _colGradients[i] += colGradients[i];
            }
        }
    }

    private static void ReadLumaRow(BitwiseImageData data, int y, int[] luma) => ReadLumaRow(data, y, luma, 0, data.w);

    /// <summary>
    /// Reads the luma for part of a row of the scanned area (i.e. offset by the padding).
    /// </summary>
    private void ReadScanRow(BitwiseImageData data, int y, int[] luma, int start, int end) =>
        ReadLumaRow(data, y, luma, start, end, _padLeft, _padTop);

    private static unsafe void ReadLumaRow(BitwiseImageData data, int y, int[] luma, int start, int end,
        int xOffset = 0, int yOffset = 0)
    {
        var row = data.ptr + data.stride * (y + yOffset);
        if (data.bytesPerPixel is 3 or 4)
        {
            for (int x = start; x < end; x++)
            {
                var pixel = row + (x + xOffset) * data.bytesPerPixel;
                int r = *(pixel + data.rOff);
                int g = *(pixel + data.gOff);
                int b = *(pixel + data.bOff);
                luma[x] = (r * R_MULT + g * G_MULT + b * B_MULT) / 1000;
            }
        }
        else if (data.bytesPerPixel == 1)
        {
            for (int x = start; x < end; x++)
            {
                luma[x] = *(row + x + xOffset);
            }
        }
        else
        {
            for (int x = start; x < end; x++)
            {
                int px = x + xOffset;
                var bit = (*(row + px / 8) >> (7 - px % 8)) & 1;
                luma[x] = bit == 0 ? 0 : 255;
            }
        }
        if (data.invertColorSpace)
        {
            for (int x = start; x < end; x++)
            {
                luma[x] = 255 - luma[x];
            }
        }
    }

    protected override void FinishCore()
    {
        int left = FindSide(_contentLeft, _colGradients, _bgLeft, _w, _h, false);
        int right = FindSide(_contentRight, _colGradients, _bgRight, _w, _h, true);
        int top = FindSide(_contentTop, _rowGradients, _bgTop, _h, _w, false);
        int bottom = FindSide(_contentBottom, _rowGradients, _bgBottom, _h, _w, true);

        if (_w - left - right < _w * MIN_PAGE_FRACTION || _h - top - bottom < _h * MIN_PAGE_FRACTION)
        {
            // Couldn't find a plausible page
            left = right = top = bottom = 0;
        }

        // The padding isn't part of the scan, so it's always cropped
        left += _padLeft;
        right += _padRight;
        top += _padTop;
        bottom += _padBottom;
        if (_invertY)
        {
            (top, bottom) = (bottom, top);
        }

        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    /// <summary>
    /// Scans inward from one side (from the end if reverse is true) over n lines of the given length, and returns the
    /// number of lines before the page starts.
    /// </summary>
    private static int FindSide(int[] content, int[] gradients, int[] bg, int n, int lineLength, bool reverse)
    {
        // Gradients look a few pixels to either side, so start a bit further in to fully ignore the outside
        int start = Math.Max((int) Math.Round(n * IGNORE_EDGE_FRACTION), 1) + EDGE_GRADIENT_SPAN;
        // Don't look further than would leave a plausible page
        int end = n - Math.Max((int) Math.Ceiling(n * MIN_PAGE_FRACTION), start);
        int sustained = Math.Max((int) Math.Round(n * SUSTAINED_CONTENT_FRACTION), EDGE_WIDTH);
        int agreement = Math.Max((int) Math.Round(n * EDGE_AGREEMENT_FRACTION), EDGE_AGREEMENT_MIN);

        var edges = new List<int>();
        var contents = new List<int>();
        for (int s = 0; s < STRIPS; s++)
        {
            int stripLength = (s + 1) * lineLength / STRIPS - s * lineLength / STRIPS;
            double minGradient = stripLength * EDGE_GRADIENT_CLAMP * MIN_EDGE_COVERAGE;
            bool HasContent(int i, double fraction) =>
                i < n && content[s * n + (reverse ? n - 1 - i : i)] >= stripLength * fraction;
            bool IsContent(int i) => HasContent(i, MIN_CONTENT_FRACTION);
            bool IsInk(int i) => HasContent(i, MIN_INK_FRACTION);
            bool IsSolid(int i) => HasContent(i, MIN_SOLID_FRACTION);
            bool IsEdge(int i) => i < n && Math.Abs(gradients[s * n + (reverse ? n - 1 - i : i)]) >= minGradient;
            // A page on a dark background always contrasts with it, so we only look for faint edges (without content)
            // on a light background. A dark background can have faint stripes that would otherwise look like edges.
            bool dark = bg[s] < DARK_BACKGROUND;

            // Edges up to here are part of an artifact from the edge of the scan area
            int skipUntil = start + EDGE_WIDTH - 1;
            for (int i = start; i < end; i++)
            {
                if (!IsContent(i) && (dark || !IsEdge(i)))
                {
                    continue;
                }
                // A page starts with an edge, which either has no ink (a blank margin) or is followed by solid content
                // (a contrasting page). Otherwise it's text or a printed line on the page. Edges may be blurry, so we
                // check for ink up to the end of the edge; a blurry printed line has ink within its edge.
                bool hasEdge = Enumerable.Range(i, EDGE_WIDTH).Any(IsEdge);
                int edgeEnd = i;
                while (edgeEnd < end && Enumerable.Range(edgeEnd + 1, EDGE_GRADIENT_SPAN + 1).Any(IsEdge)) edgeEnd++;
                if (i <= skipUntil && IsEdge(i) && !IsContent(i))
                {
                    // An edge that's there when we start (or soon after the last one) is part of an artifact from the
                    // edge of the scan area (e.g. a dark border that gradually fades out), so skip past it
                    skipUntil = edgeEnd + agreement;
                    i = edgeEnd;
                    continue;
                }
                bool blankMargin = !Enumerable.Range(i, edgeEnd - i + EDGE_GRADIENT_SPAN + 1).Any(IsInk);
                int solidStart = Enumerable.Range(i, edgeEnd - i + agreement).FirstOrDefault(IsSolid, -1);
                bool contrastingPage = solidStart != -1 && Enumerable.Range(solidStart, sustained).All(IsSolid);
                if (hasEdge && (contrastingPage || blankMargin && !dark))
                {
                    // The page starts after any shadow/highlight/blur around the edge. That's either where solid
                    // content starts, or the end of the edge (adjusted as the gradient looks behind by the span).
                    edges.Add(contrastingPage ? solidStart : edgeEnd - EDGE_GRADIENT_SPAN + 1);
                }
                else
                {
                    // The gradient at a line looks ahead by the gradient span, so that's where the content starts
                    contents.Add(IsEdge(i) ? i + EDGE_GRADIENT_SPAN : i);
                }
                break;
            }
        }

        // Find the outermost edge that's confirmed by another strip
        edges.Sort();
        for (int i = 0; i < edges.Count - 1; i++)
        {
            if (edges[i + 1] - edges[i] > agreement) continue;
            int edge = edges[i];
            if (contents.Count(x => Math.Abs(x - edge) > agreement) >= edges.Count(x => x <= edge + agreement))
            {
                // As many strips found content elsewhere (without an edge before it) as found this edge, so it's
                // probably not the edge of the page (e.g. it's a faint printed line, or part of a photo)
                return 0;
            }
            return edge;
        }
        return 0;
    }
}
