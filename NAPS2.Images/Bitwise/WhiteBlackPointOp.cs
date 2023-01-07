namespace NAPS2.Images.Bitwise;

/// <summary>
/// Corrects images with poor calibration for white/black values.
/// </summary>
public class WhiteBlackPointOp : UnaryBitwiseImageOp
{
    // When we've identified the block of pixel values that we consider white (or black),
    // this is the percentile (counting from the mid levels) at which we set the
    // actual white/black point. 0 is maximally aggressive correction and flattens
    // out near-white and near-black colors to pure white/black, while 1 leaves all
    // of the variability/noise.
    private const double PERCENTILE = 0.2;

    /// <summary>
    /// Performs this operation including pre-processing steps.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="mode"></param>
    public static void PerformFullOp(IMemoryImage image, CorrectionMode mode)
    {
        var op1 = new WhiteBlackPointPreOp(mode);
        op1.Perform(image);
        new WhiteBlackPointOp(op1).Perform(image);
    }

    private readonly CorrectionMode _mode;
    private readonly bool _valid;
    private readonly int _whitePoint;
    private readonly int _blackPoint;

    public WhiteBlackPointOp(WhiteBlackPointPreOp preOp)
    {
        _mode = preOp.Mode;

        var counts = preOp.PixelLumCounts;
        var total = preOp.PixelTotalCount;

        var peaks = FindPeaks(counts, total);
        var whitePeak = peaks.OrderByDescending(WhitePeakScore).FirstOrDefault();
        var blackPeak = peaks.OrderByDescending(BlackPeakScore).FirstOrDefault();
        if (whitePeak == null || blackPeak == null || whitePeak.Value <= blackPeak.Value)
        {
            _valid = false;
            return;
        }

        _whitePoint = GetWhitePoint(counts, whitePeak);
        _blackPoint = GetBlackPoint(counts, blackPeak);
        _valid = true;

        Console.WriteLine($"Correcting with whitepoint {_whitePoint} blackpoint {_blackPoint}");
    }

    private static int GetWhitePoint(int[] counts, Peak whitePeak)
    {
        var whiteTotal = counts.Skip(whitePeak.Left).Sum();
        long whiteCumul = 0;
        var whitePoint = whitePeak.Right;
        for (int i = whitePeak.Left; i < whitePeak.Right; i++)
        {
            whiteCumul += counts[i];
            if (whiteCumul >= PERCENTILE * whiteTotal)
            {
                whitePoint = i;
                break;
            }
        }
        return whitePoint;
    }

    private static int GetBlackPoint(int[] counts, Peak blackPeak)
    {
        var blackTotal = counts.Take(blackPeak.Right + 1).Sum();
        long blackCumul = 0;
        var blackPoint = blackPeak.Left;
        for (int i = blackPeak.Right; i > blackPeak.Left; i--)
        {
            blackCumul += counts[i];
            if (blackCumul >= PERCENTILE * blackTotal)
            {
                blackPoint = i;
                break;
            }
        }
        return blackPoint;
    }

    private double BlackPeakScore(Peak peak)
    {
        var vScore = Math.Pow(1 - peak.Value / 255.0, 3);
        var hScore = Magnitude(peak.Height) - Magnitude(Math.Min(peak.LeftBottom, peak.RightBottom));
        return vScore * hScore;
    }

    private double Magnitude(double h)
    {
        return Math.Log10(1e4 * h + 1);
    }

    private double WhitePeakScore(Peak peak)
    {
        var vScore = Math.Pow(peak.Value / 255.0, 3);
        var mScore = peak.Mass > 0.1 ? Math.Log10(100 * peak.Mass) : 10 * peak.Mass;
        return vScore * mScore;
    }

    private static List<Peak> FindPeaks(int[] counts, long total)
    {
        var peaks = new List<Peak>();
        var dCounts = counts.Select(x => x / (double) total).ToList();
        var dCountsPlus = new double[] { 0, 0 }.Concat(dCounts).Concat(new double[] { 0, 0 }).ToList();
        for (int i = 0; i < 256; i++)
        {
            if (dCounts[i] > dCountsPlus[i + 1] && dCounts[i] > dCountsPlus[i] &&
                dCounts[i] > dCountsPlus[i + 4] && dCounts[i] > dCountsPlus[i + 3])
            {
                var p = new Peak
                {
                    Value = i,
                    Height = dCounts[i],
                    LeftBottom = dCounts[i],
                    RightBottom = dCounts[i],
                    Left = i,
                    Right = i,
                    Mass = dCounts[i]
                };
                for (int j = i - 1; j >= 0; j--)
                {
                    if (dCounts[j] < p.LeftBottom || j > 0 && dCounts[j - 1] < p.LeftBottom)
                    {
                        p.LeftBottom = Math.Min(p.LeftBottom, dCounts[j]);
                        p.Left = j;
                        p.Mass += dCounts[j];
                    }
                    else
                    {
                        break;
                    }
                }
                for (int j = i + 1; j < 256; j++)
                {
                    if (dCounts[j] < p.RightBottom || j < 255 && dCounts[j + 1] < p.RightBottom)
                    {
                        p.RightBottom = Math.Min(p.RightBottom, dCounts[j]);
                        p.Right = j;
                        p.Mass += dCounts[j];
                    }
                    else
                    {
                        break;
                    }
                }
                peaks.Add(p);
            }
        }
        return peaks;
    }

    protected override unsafe void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (!_valid)
            return;
        bool flatten = _mode == CorrectionMode.Document;
        bool retainColor = _mode == CorrectionMode.Photo;
        var iToL = GammaTables.IntensityToLum;
        var lToI = GammaTables.LumToIntensity;
        var blackL = iToL[_blackPoint];
        var whiteL = iToL[_whitePoint];
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                int r = *(pixel + data.rOff);
                int g = *(pixel + data.gOff);
                int b = *(pixel + data.bOff);

                if (flatten)
                {
                    var white = _whitePoint;
                    var black = _blackPoint;

                    if (r < black)
                        r = black;
                    if (g < black)
                        g = black;
                    if (b < black)
                        b = black;

                    if (r > white)
                        r = white;
                    if (g > white)
                        g = white;
                    if (b > white)
                        b = white;

                    // Use a gamma function to convert to the luminescence space
                    int rL = iToL[r];
                    int gL = iToL[g];
                    int bL = iToL[b];
                    // Scale the color values in the luminescence space
                    rL = (rL - blackL) * GammaTables.MAX_LUM / (whiteL - blackL);
                    gL = (gL - blackL) * GammaTables.MAX_LUM / (whiteL - blackL);
                    bL = (bL - blackL) * GammaTables.MAX_LUM / (whiteL - blackL);
                    // Convert back to the intensity space
                    r = lToI[rL];
                    g = lToI[gL];
                    b = lToI[bL];
                }

                if (retainColor)
                {
                    var min = Math.Min(r, Math.Min(g, b));
                    var max = Math.Max(r, Math.Max(g, b));
                    var black = Math.Min(min, _blackPoint);
                    var white = Math.Max(max, _whitePoint);

                    r = (r - black) * 255 / (white - black);
                    g = (g - black) * 255 / (white - black);
                    b = (b - black) * 255 / (white - black);
                }

                *(pixel + data.rOff) = (byte) r;
                *(pixel + data.gOff) = (byte) g;
                *(pixel + data.bOff) = (byte) b;
            }
        }
    }

    private class Peak
    {
        public int Value { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public double Height { get; set; }
        public double LeftBottom { get; set; }
        public double RightBottom { get; set; }
        public double Mass { get; set; }
    }
}