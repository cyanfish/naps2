namespace NAPS2.ImportExport;

internal static class SaveSeparatorHelper
{
    /// <summary>
    /// Given a list of scans (each of which is a list of 1 or more images),
    /// split up the images into multiple lists as described by the SaveSeparator parameter.
    /// </summary>
    /// <param name="scans"></param>
    /// <param name="separator"></param>
    /// <param name="splitSize"></param>
    /// <returns></returns>
    public static IEnumerable<List<ProcessedImage>> SeparateScans(IEnumerable<IEnumerable<ProcessedImage>> scans,
        SaveSeparator separator, int splitSize = 1, BarcodeSeparatorOptions? barcodeSeparatorOptions = null)
    {
        return SeparateScansWithBarcodeValues(scans, separator, splitSize, barcodeSeparatorOptions)
            .Select(x => x.Images);
    }

    public static IEnumerable<(List<ProcessedImage> Images, string? BarcodeValue)> SeparateScansWithBarcodeValues(
        IEnumerable<IEnumerable<ProcessedImage>> scans, SaveSeparator separator, int splitSize = 1,
        BarcodeSeparatorOptions? barcodeSeparatorOptions = null, bool removePatchTPage = true,
        bool removePatchCodePages = false)
    {
        if (separator == SaveSeparator.FilePerScan)
        {
            foreach (var scan in scans)
            {
                var images = scan.ToList();
                if (images.Count > 0)
                {
                    yield return (images, null);
                }
            }
        }
        else if (separator == SaveSeparator.FilePerPage)
        {
            splitSize = Math.Max(splitSize, 1);
            foreach (var scan in scans.Select(x => x.ToList()))
            {
                for (int i = 0; i < scan.Count; i += splitSize)
                {
                    yield return (scan.Skip(i).Take(splitSize).ToList(), null);
                }
            }
        }
        else if (separator == SaveSeparator.PatchT)
        {
            var images = new List<ProcessedImage>();
            string? barcodeValue = "";
            foreach (var scan in scans)
            {
                foreach (var image in scan)
                {
                    if (image.PostProcessingData.Barcode.IsPatchT)
                    {
                        if (images.Count > 0)
                        {
                            yield return (images, barcodeValue);
                            images = [];
                        }
                        barcodeValue = image.PostProcessingData.Barcode.DetectedText;
                        if (removePatchTPage)
                        {
                            image.Dispose();
                        }
                        else
                        {
                            images.Add(image);
                        }
                    }
                    else
                    {
                        images.Add(image);
                    }
                }
            }
            if (images.Count > 0)
            {
                yield return (images, barcodeValue);
            }
        }
        else if (separator == SaveSeparator.Barcode)
        {
            barcodeSeparatorOptions ??= new BarcodeSeparatorOptions();
            barcodeSeparatorOptions.Validate();
            var images = new List<ProcessedImage>();
            string? barcodeValue = null;
            foreach (var scan in scans)
            {
                foreach (var image in scan)
                {
                    var barcode = image.PostProcessingData.Barcode;
                    if (barcodeSeparatorOptions.Matches(barcode))
                    {
                        if (images.Count > 0)
                        {
                            yield return (images, barcodeValue ?? "");
                            images = [];
                        }
                        barcodeValue = barcode.DetectedText;
                        if (barcodeSeparatorOptions.RemoveBarcodePage ||
                            removePatchCodePages && barcode.PatchCode != null)
                        {
                            image.Dispose();
                        }
                        else
                        {
                            images.Add(image);
                        }
                    }
                    else
                    {
                        if (barcodeValue == null && barcodeSeparatorOptions.CanUseForFilename(barcode))
                        {
                            barcodeValue = barcode.DetectedText;
                        }
                        images.Add(image);
                    }
                }
            }
            if (images.Count > 0)
            {
                yield return (images, barcodeValue ?? "");
            }
        }
        else
        {
            var images = scans.SelectMany(x => x.ToList()).ToList();
            if (images.Count > 0)
            {
                yield return (images, null);
            }
        }
    }
}