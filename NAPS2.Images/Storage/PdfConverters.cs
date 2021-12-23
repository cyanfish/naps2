// using PdfSharp.Pdf;
//
// namespace NAPS2.Images.Storage;
//
// public class PdfConverters
// {
//     private readonly ImageContext _imageContext;
//
//     public PdfConverters(ImageContext imageContext)
//     {
//         _imageContext = imageContext;
//     }
//
//     [StorageConverter]
//     public FileStorage ConvertToFile(PdfStorage input, StorageConvertParams convertParams)
//     {
//         var path = convertParams.Temporary
//             ? Path.Combine(Paths.Temp, Path.GetRandomFileName())
//             : _imageContext.FileStorageManager.NextFilePath() + ".pdf";
//         input.Document.Save(path);
//         return new FileStorage(path);
//     }
//
//     [StorageConverter]
//     public PdfStorage ConvertToMemory(FileStorage input, StorageConvertParams convertParams) => new PdfStorage(new PdfDocument(input.FullPath));
// }