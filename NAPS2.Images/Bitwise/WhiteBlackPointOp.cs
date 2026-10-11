namespace NAPS2.Images.Bitwise;

/// <summary>
/// Corrects images with poor calibration for white/black values.
/// </summary>
internal class WhiteBlackPointOp : UnaryBitwiseImageOp
{
    // When we've identified the block of pixel values that we consider white (or black),
    // this is the percentile (counting from the mid levels) at which we set the
    // actual white/black point. 0 is maximally aggressive correction and flattens
    // out near-white and near-black colors to pure white/black, while 1 leaves all
    // of the variability/noise.
    private const double PERCENTILE = 0.2;

    // If the paper color has a channel spread (max - min) larger than this, we consider it to be tinted (e.g. yellowed
    // or colored paper). Stretching each channel to the same white point would amplify the tint, so instead we only
    // correct the black point and leave the paper color as-is.
    private const int MAX_PAPER_TINT = 16;

    // Stretching each channel separately amplifies small color differences, e.g. the colored fringes that scanners
    // produce around text and the page edges (from the color channels being slightly misaligned). So for pixels with
    // low chroma (max - min channel value) we instead stretch just the luminance and scale down the chroma based on
    // how close the pixel gets to black/white. Between these two chroma values we blend into the per-channel result,
    // so that genuinely colored content (charts, highlighters, etc.) keeps its color.
    private const int CHROMA_BLEND_MIN = 16;
    private const int CHROMA_BLEND_MAX = 48;

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

    // Per-channel lookup tables (Document mode only)
    private readonly byte[]? _lumTable;
    private readonly byte[]? _rTable;
    private readonly byte[]? _gTable;
    private readonly byte[]? _bTable;
    private readonly int[]? _chromaScales;

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
        if (_whitePoint <= _blackPoint)
        {
            _valid = false;
            return;
        }
        if (_mode == CorrectionMode.Document)
        {
            var (paperR, paperG, paperB) = GetPaperColor(preOp, whitePeak);
            var paperMax = Math.Max(paperR, Math.Max(paperG, paperB));
            var paperMin = Math.Min(paperR, Math.Min(paperG, paperB));
            if (paperMax - paperMin > MAX_PAPER_TINT)
            {
                // Preserve tinted paper color (rather than doing a correction that's likely to result in an oversaturated background) 
                var paperLum = Math.Max((paperR * R_MULT + paperG * G_MULT + paperB * B_MULT) / 1000, 1);
                var whiteR = Math.Min(paperR * _whitePoint / paperLum, 255);
                var whiteG = Math.Min(paperG * _whitePoint / paperLum, 255);
                var whiteB = Math.Min(paperB * _whitePoint / paperLum, 255);
                if (whiteR <= _blackPoint || whiteG <= _blackPoint || whiteB <= _blackPoint)
                {
                    // The paper is so strongly colored that one channel is darker than the black point, so we can't
                    // correct it without shifting the colors
                    _valid = false;
                    return;
                }
                _rTable = BuildColorChannelLookupTable(_blackPoint, whiteR, whiteR);
                _gTable = BuildColorChannelLookupTable(_blackPoint, whiteG, whiteG);
                _bTable = BuildColorChannelLookupTable(_blackPoint, whiteB, whiteB);
            }
            else
            {
                // Flatten everything at or above the white point to pure white
                var table = BuildColorChannelLookupTable(_blackPoint, _whitePoint, 255);
                _rTable = _gTable = _bTable = _lumTable = table;
                _chromaScales = BuildChromaScales(table);
            }
        }
        _valid = true;

        // Console.WriteLine($"Correcting with whitepoint {_whitePoint} blackpoint {_blackPoint}");
    }

    private static (int r, int g, int b) GetPaperColor(WhiteBlackPointPreOp preOp, Peak whitePeak)
    {
        long count = 0, rSum = 0, gSum = 0, bSum = 0;
        for (int i = whitePeak.Left; i < 256; i++)
        {
            count += preOp.PixelLumCounts[i];
            rSum += preOp.PixelRSums[i];
            gSum += preOp.PixelGSums[i];
            bSum += preOp.PixelBSums[i];
        }
        if (count == 0)
        {
            return (255, 255, 255);
        }
        return ((int) (rSum / count), (int) (gSum / count), (int) (bSum / count));
    }

    /// <summary>
    /// For each luminance value, gets how much the lookup table moves it toward black or white, as a scale factor
    /// (x256) for the pixel's chroma. E.g. luminance mapped to pure white or black gets 0 (no chroma).
    /// </summary>
    private static int[] BuildChromaScales(byte[] lumTable)
    {
        var scales = new int[256];
        for (int lum = 0; lum < 256; lum++)
        {
            int newLum = lumTable[lum];
            scales[lum] = newLum < lum ? newLum * 256 / lum
                : newLum > lum ? (255 - newLum) * 256 / (255 - lum)
                : 256;
        }
        return scales;
    }

    private static byte[] BuildColorChannelLookupTable(int blackPoint, int whitePoint, int target)
    {
        var table = new byte[256];
        var iToL = GammaTables.IntensityToLum;
        var lToI = GammaTables.LumToIntensity;
        var blackL = iToL[blackPoint];
        var whiteL = iToL[whitePoint];
        var targetL = iToL[target];
        for (int i = 0; i < 256; i++)
        {
            if (i < whitePoint)
            {
                // Use a gamma function to convert to the luminescence space, scale the value, then convert back to the
                // intensity space
                var l = iToL[Math.Max(i, blackPoint)];
                table[i] = lToI[(l - blackL) * targetL / (whiteL - blackL)];
            }
            else if (whitePoint == 255)
            {
                table[i] = (byte) target;
            }
            else
            {
                table[i] = (byte) (target + (i - whitePoint) * (255 - target) / (255 - whitePoint));
            }
        }
        return table;
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
        var rTable = _rTable;
        var gTable = _gTable;
        var bTable = _bTable;
        var lumTable = _lumTable;
        var chromaScales = _chromaScales;
        bool flatten = _mode == CorrectionMode.Document;
        bool retainColor = _mode == CorrectionMode.Photo;
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
                    int chroma = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
                    if (lumTable != null && chroma < CHROMA_BLEND_MAX)
                    {
                        // Stretch the luminance and scale the chroma (see CHROMA_BLEND_MIN/MAX)
                        int lum = (r * R_MULT + g * G_MULT + b * B_MULT) / 1000;
                        int newLum = lumTable[lum];
                        int scale = chromaScales![lum];
                        int lumR = Math.Clamp(newLum + ((r - lum) * scale >> 8), 0, 255);
                        int lumG = Math.Clamp(newLum + ((g - lum) * scale >> 8), 0, 255);
                        int lumB = Math.Clamp(newLum + ((b - lum) * scale >> 8), 0, 255);
                        // Blend with the per-channel result based on the chroma (weight x256)
                        int weight = Math.Max(chroma - CHROMA_BLEND_MIN, 0) * 256 /
                                     (CHROMA_BLEND_MAX - CHROMA_BLEND_MIN);
                        r = (lumR * (256 - weight) + rTable![r] * weight) >> 8;
                        g = (lumG * (256 - weight) + gTable![g] * weight) >> 8;
                        b = (lumB * (256 - weight) + bTable![b] * weight) >> 8;
                    }
                    else
                    {
                        r = rTable![r];
                        g = gTable![g];
                        b = bTable![b];
                    }
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