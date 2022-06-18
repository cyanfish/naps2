﻿// using NAPS2.Dependencies;
//
// namespace NAPS2.Ocr;
//
// public class TesseractSystemData : TesseractOcrEngineData
// {
//     private bool _isInstalled;
//     private DateTime? _installCheckTime;
//     private List<Language>? _installedLanguages;
//
//     public TesseractSystemData()
//     {
//         // Use the most complete set of language mappings
//         LanguageData = TesseractLanguageData.V400B4;
//         TesseractBasePath = "";
//         TesseractExePath = "tesseract";
//         PlatformSupport = PlatformSupport.Linux;
//         CanInstall = false;
//     }
//
//     public override TesseractRunInfo TesseractRunInfo(OcrParams ocrParams) => new()
//     {
//         Arguments = "",
//         DataPath = null,
//         PrefixPath = null
//     };
//
//     public override bool IsInstalled
//     {
//         get
//         {
//             CheckIfInstalled();
//             return _isInstalled;
//         }
//     }
//
//     public override IEnumerable<Language> InstalledLanguages
//     {
//         get
//         {
//             CheckIfInstalled();
//             return _installedLanguages ?? Enumerable.Empty<Language>();
//         }
//     }
//
//     public override IEnumerable<Language> NotInstalledLanguages => Enumerable.Empty<Language>();
//
//     private void CheckIfInstalled()
//     {
//         if (IsSupported && (_installCheckTime == null || _installCheckTime < DateTime.Now - TimeSpan.FromSeconds(2)))
//         {
//             try
//             {
//                 var process = Process.Start(new ProcessStartInfo
//                 {
//                     FileName = TesseractExePath,
//                     Arguments = "--list-langs",
//                     UseShellExecute = false,
//                     RedirectStandardOutput = true,
//                     RedirectStandardError = true
//                 });
//                 if (process != null && process.Id != 0)
//                 {
//                     var codes = process.StandardError.ReadToEnd().Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length == 3);
//                     _installedLanguages = codes.Select(code => LanguageData.LanguageMap.Get($"ocr-{code}")).WhereNotNull().ToList();
//                     _isInstalled = true;
//                     process.Kill();
//                 }
//             }
//             catch (Exception)
//             {
//                 // Component is not installed on the system path (or had an error)
//             }
//             _installCheckTime = DateTime.Now;
//         }
//     }
// }