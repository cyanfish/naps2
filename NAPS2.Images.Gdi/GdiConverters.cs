// using System.Drawing;
// using System.Drawing.Imaging;
//
// namespace NAPS2.Images.Gdi;
//
// public class GdiConverters
// {
//     [StorageConverter]
//     public FileStorage ConvertToFile(GdiImage input, StorageConvertParams convertParams)
//     {
//         if (convertParams.Temporary)
//         {
//             var path = Path.Combine(Paths.Temp, Path.GetRandomFileName());
//             input.Bitmap.Save(path);
//             return new FileStorage(path);
//         }
//         else
//         {
//             var tempPath = ScannedImageHelper.SaveSmallestBitmap(input.Bitmap, convertParams.BitDepth, convertParams.Lossless, convertParams.LossyQuality, out ImageFormat fileFormat);
//             string ext = Equals(fileFormat, ImageFormat.Png) ? ".png" : ".jpg";
//             var path = _imageContext.FileStorageManager.NextFilePath() + ext;
//             File.Move(tempPath, path);
//             return new FileStorage(path);
//         }
//     }
//
//     [StorageConverter]
//     public GdiImage ConvertToGdi(FileStorage input, StorageConvertParams convertParams)
//     {
//         // TODO: Allow multiple converters (with priority?) and fall back to the next if it returns null
//         // Then we can have a PDF->Image converter that returns null if it's not a pdf file.
//         if (IsPdfFile(input))
//         {
//             return (GdiImage)_imageContext.PdfRenderer.Render(input.FullPath, 300).Single();
//         }
//         else
//         {
//             return new GdiImage(new Bitmap(input.FullPath));
//         }
//     }
//
//     private static bool IsPdfFile(FileStorage fileStorage) => Path.GetExtension(fileStorage.FullPath)?.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) ?? false;
//
// }