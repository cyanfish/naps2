using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public abstract class TesseractBaseEngine : IOcrEngine
    {
        private const int DEFAULT_TIMEOUT = 120 * 1000;
        private const int CHECK_INTERVAL = 500;

        private readonly AppConfigManager appConfigManager;

        protected TesseractBaseEngine(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
        }

        public bool CanProcess(string langCode)
        {
            if (string.IsNullOrEmpty(langCode) || !IsInstalled || !IsSupported)
            {
                return false;
            }
            // Support multiple specified languages (e.g. "eng+fra")
            return langCode.Split('+').All(code => InstalledLanguages.Any(x => x.Code == code));
        }

        public OcrResult ProcessImage(string imagePath, OcrParams ocrParams, Func<bool> cancelCallback)
        {
            string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            string tempHocrFilePathWithExt = tempHocrFilePath + TesseractHocrExtension;
            try
            {
                var runInfo = TesseractRunInfo(ocrParams);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(TesseractBasePath, TesseractExePath),
                    Arguments = $"\"{imagePath}\" \"{tempHocrFilePath}\" -l {ocrParams.LanguageCode} {runInfo.Arguments} hocr",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                if (runInfo.PrefixPath != null)
                {
                    startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = Path.Combine(TesseractBasePath, runInfo.PrefixPath);
                }
                if (runInfo.DataPath != null)
                {
                    var tessdata = new DirectoryInfo(Path.Combine(TesseractBasePath, runInfo.DataPath));
                    EnsureHocrConfigExists(tessdata);
                }
                var tesseractProcess = Process.Start(startInfo);
                if (tesseractProcess == null)
                {
                    // Couldn't start tesseract for some reason
                    Log.Error("Couldn't start OCR process.");
                    return null;
                }
                var timeout = (int)(appConfigManager.Config.OcrTimeoutInSeconds * 1000);
                if (timeout == 0)
                {
                    timeout = DEFAULT_TIMEOUT;
                }
                var stopwatch = Stopwatch.StartNew();
                while (!tesseractProcess.WaitForExit(CHECK_INTERVAL))
                {
                    if (stopwatch.ElapsedMilliseconds >= timeout || cancelCallback())
                    {
                        if (stopwatch.ElapsedMilliseconds >= timeout)
                        {
                            Log.Error("OCR process timed out.");
                        }
                        try
                        {
                            tesseractProcess.Kill();
                            // Wait a bit to give the process time to release its file handles
                            Thread.Sleep(200);
                        }
                        catch (Exception e)
                        {
                            Log.ErrorException("Error killing OCR process", e);
                        }
                        return null;
                    }
                }
#if DEBUG
                var output = tesseractProcess.StandardOutput.ReadToEnd();
                if (output.Length > 0)
                {
                    Log.Error("Tesseract stdout: {0}", output);
                }
                output = tesseractProcess.StandardError.ReadToEnd();
                if (output.Length > 0)
                {
                    Log.Error("Tesseract stderr: {0}", output);
                }
#endif
                XDocument hocrDocument = XDocument.Load(tempHocrFilePathWithExt);
                return new OcrResult
                {
                    PageBounds = hocrDocument.Descendants()
                        .Where(x => x.Attributes("class").Any(y => y.Value == "ocr_page"))
                        .Select(x => GetBounds(x.Attribute("title")))
                        .First(),
                    Elements = hocrDocument.Descendants()
                        .Where(x => x.Attributes("class").Any(y => y.Value == "ocrx_word"))
                        .Select(x => new OcrResultElement { Text = x.Value, Bounds = GetBounds(x.Attribute("title")) }),
                    RightToLeft = InstalledLanguages.Where(x => x.Code == ocrParams.LanguageCode).Select(x => x.RTL).FirstOrDefault()
                };
            }
            catch (Exception e)
            {
                Log.ErrorException("Error running OCR", e);
                return null;
            }
            finally
            {
                try
                {
                    File.Delete(tempHocrFilePathWithExt);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error cleaning up OCR temp files", e);
                }
            }
        }

        private void EnsureHocrConfigExists(DirectoryInfo tessdata)
        {
            var configDir = new DirectoryInfo(Path.Combine(tessdata.FullName, "configs"));
            if (!configDir.Exists)
            {
                configDir.Create();
            }
            var hocrConfigFile = new FileInfo(Path.Combine(configDir.FullName, "hocr"));
            if (!hocrConfigFile.Exists)
            {
                using (var writer = hocrConfigFile.CreateText())
                {
                    writer.Write("tessedit_create_hocr 1");
                }
            }
        }

        private Rectangle GetBounds(XAttribute titleAttr)
        {
            var bounds = new Rectangle();
            if (titleAttr != null)
            {
                foreach (var param in titleAttr.Value.Split(';'))
                {
                    string[] parts = param.Trim().Split(' ');
                    if (parts.Length == 5 && parts[0] == "bbox")
                    {
                        int x1 = int.Parse(parts[1]), y1 = int.Parse(parts[2]);
                        int x2 = int.Parse(parts[3]), y2 = int.Parse(parts[4]);
                        bounds = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    }
                }
            }
            return bounds;
        }

        protected abstract string TesseractBasePath { get; }

        protected abstract string TesseractExePath { get; }

        protected abstract string TesseractHocrExtension { get; }

        protected virtual RunInfo TesseractRunInfo(OcrParams ocrParams) => new RunInfo
        {
            Arguments = "",
            DataPath = "tessdata",
            PrefixPath = ""
        };

        protected virtual DownloadInfo DownloadInfo => null;

        protected abstract PlatformSupport PlatformSupport { get; }

        public virtual bool IsSupported => PlatformSupport.Validate();

        public virtual bool IsInstalled => Component.IsInstalled;

        public abstract bool IsUpgradable { get; }

        public abstract bool CanInstall { get; }

        public virtual IEnumerable<Language> InstalledLanguages => LanguageComponents.Where(x => x.IsInstalled).Select(x => Languages[x.Id]);

        public virtual IEnumerable<Language> NotInstalledLanguages => LanguageComponents.Where(x => !x.IsInstalled).Select(x => Languages[x.Id]);

        public virtual IExternalComponent Component => new ExternalComponent("ocr", Path.Combine(TesseractBasePath, TesseractExePath), DownloadInfo);

        public virtual IEnumerable<IExternalComponent> LanguageComponents => TesseractLanguageData.Select(x =>
            new ExternalComponent($"ocr-{x.Code}", Path.Combine(TesseractBasePath, "tessdata", x.Filename.Replace(".zip", "")),
                CanInstall ? new DownloadInfo(x.Filename, TesseractMirrors, x.Size, x.Sha1, DownloadFormat.Zip) : null));

        public virtual IEnumerable<OcrMode> SupportedModes => null;

        protected static readonly List<DownloadMirror> TesseractMirrors = new List<DownloadMirror>
        {
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.00b4/{0}"),
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.00b4/{0}/download"),
            new DownloadMirror(PlatformSupport.WindowsXp, @"http://xp-mirror.naps2.com/tesseract-4.00b4/{0}")
        };

        protected class RunInfo
        {
            public string Arguments { get; set; }

            public string PrefixPath { get; set; }

            public string DataPath { get; set; }
        }

        protected class TesseractLanguage
        {
            public string Filename { get; set; }

            public string Code { get; set; }

            public string LangName { get; set; }

            public double Size { get; set; }

            public string Sha1 { get; set; }

            public bool RTL { get; set; }
        }

        protected readonly Dictionary<string, Language> Languages = TesseractLanguageData.ToDictionary(x => $"ocr-{x.Code}", x => new Language(x.Code, x.LangName, x.RTL));

        #region Language Data (auto-generated)

        protected static readonly TesseractLanguage[] TesseractLanguageData =
        {
            new TesseractLanguage { Filename = "afr.traineddata.zip", Code = "afr", LangName = "Afrikaans", Size = 5.44, Sha1 = "4278120a18e3464194df302f55417afc35415af7" },
            new TesseractLanguage { Filename = "amh.traineddata.zip", Code = "amh", LangName = "Amharic", Size = 5.55, Sha1 = "166219c79a3c92775ac8cc987fba91899dc63f7d" },
            new TesseractLanguage { Filename = "ara.traineddata.zip", Code = "ara", LangName = "Arabic", Size = 2.29, Sha1 = "6a09f2f96ee04d2bf1c887ea10bcaace429908a6", RTL = true },
            new TesseractLanguage { Filename = "asm.traineddata.zip", Code = "asm", LangName = "Assamese", Size = 2.82, Sha1 = "fe4b1d832af281a7947ccf86300c9574827d3b50" },
            new TesseractLanguage { Filename = "aze.traineddata.zip", Code = "aze", LangName = "Azerbaijani", Size = 5.69, Sha1 = "aa1092d2931dc0d500bda58987a49d3a9bb6d98d", RTL = true },
            new TesseractLanguage { Filename = "aze_cyrl.traineddata.zip", Code = "aze_cyrl", LangName = "Azerbaijani (Cyrillic)", Size = 2.88, Sha1 = "72fd2b3e1d6f3c88b09f238306e89742b2ae6f0f" },
            new TesseractLanguage { Filename = "bel.traineddata.zip", Code = "bel", LangName = "Belarusian", Size = 5.86, Sha1 = "80da0e84413031213eb30c8b4064fb25b2913cad" },
            new TesseractLanguage { Filename = "ben.traineddata.zip", Code = "ben", LangName = "Bengali", Size = 1.84, Sha1 = "b89191580d742a688bf435fa9ff94f9d468c4858" },
            new TesseractLanguage { Filename = "bod.traineddata.zip", Code = "bod", LangName = "Tibetan", Size = 2.44, Sha1 = "45c144a9d5bf1cdbec50fc24bd3d49bb8f9eba95" },
            new TesseractLanguage { Filename = "bos.traineddata.zip", Code = "bos", LangName = "Bosnian", Size = 4.08, Sha1 = "412d5fff06e9faee873e19cfac0d9e6c72a3c7c8" },
            new TesseractLanguage { Filename = "bre.traineddata.zip", Code = "bre", LangName = "Breton", Size = 6.46, Sha1 = "124f6cdd9b44fb49783fb9908777216d5308102e" },
            new TesseractLanguage { Filename = "bul.traineddata.zip", Code = "bul", LangName = "Bulgarian", Size = 4.32, Sha1 = "05be97ef3169fd953175e37e0b91390a39e5c198" },
            new TesseractLanguage { Filename = "cat.traineddata.zip", Code = "cat", LangName = "Catalan", Size = 3.20, Sha1 = "b8cb54105535c07dd4fd7b9aec441cc27f6692f8" },
            new TesseractLanguage { Filename = "ceb.traineddata.zip", Code = "ceb", LangName = "Cebuano", Size = 1.52, Sha1 = "484d250a6863e8e1fed00368f6a62c049b5b972c" },
            new TesseractLanguage { Filename = "ces.traineddata.zip", Code = "ces", LangName = "Czech", Size = 8.43, Sha1 = "69a7b67e1175ccecd39882e7d521872a93ee85c7" },
            new TesseractLanguage { Filename = "chi_sim.traineddata.zip", Code = "chi_sim", LangName = "Chinese (Simplified)", Size = 20.73, Sha1 = "e26f943534443c274c43a81b24ac10f4c277b9e3" },
            new TesseractLanguage { Filename = "chi_sim_vert.traineddata.zip", Code = "chi_sim_vert", LangName = "Chinese (Simplified, Vertical)", Size = 2.81, Sha1 = "898de9e4322bf818ae7e94cbd89834a9ac0fc7e9" },
            new TesseractLanguage { Filename = "chi_tra.traineddata.zip", Code = "chi_tra", LangName = "Chinese (Traditional)", Size = 27.26, Sha1 = "0eb485e9961bad5f4fe1237b13f61165c356532f" },
            new TesseractLanguage { Filename = "chi_tra_vert.traineddata.zip", Code = "chi_tra_vert", LangName = "Chinese (Traditional, Vertical)", Size = 2.70, Sha1 = "dbefb7180af04cf64b630473dd0ed70569369c1b" },
            new TesseractLanguage { Filename = "chr.traineddata.zip", Code = "chr", LangName = "Cherokee", Size = 0.92, Sha1 = "387d14e948dafe053c644e60a2429f4523e743a3" },
            new TesseractLanguage { Filename = "cos.traineddata.zip", Code = "cos", LangName = "Corsican", Size = 2.64, Sha1 = "892ec8f2156de1d1dd1812b730cbaea59f645097" },
            new TesseractLanguage { Filename = "cym.traineddata.zip", Code = "cym", LangName = "Welsh", Size = 3.97, Sha1 = "42db973712f4949012405295af0cd5eac09b73df" },
            new TesseractLanguage { Filename = "dan.traineddata.zip", Code = "dan", LangName = "Danish", Size = 5.72, Sha1 = "62b39c7b7eb560f2b1910b7e2abbcf62c5b9c882" },
            new TesseractLanguage { Filename = "dan_frak.traineddata.zip", Code = "dan_frak", LangName = "Danish (Fraktur)", Size = 0.65, Sha1 = "becc87d384ddc8f410d5d68ef8c2644bd79fa2ee" },
            new TesseractLanguage { Filename = "deu.traineddata.zip", Code = "deu", LangName = "German", Size = 7.58, Sha1 = "22566b9236a55c3f93324ac78ce26a09c0c4aecc" },
            new TesseractLanguage { Filename = "deu_frak.traineddata.zip", Code = "deu_frak", LangName = "German (Fraktur)", Size = 0.78, Sha1 = "5cd0fbf328e0c6c99f3e7bdd0b8b79ac78166f58" },
            new TesseractLanguage { Filename = "div.traineddata.zip", Code = "div", LangName = "Maldivian", Size = 1.70, Sha1 = "ac91a1e0c11529e958c033394d39ca727fd72091", RTL = true },
            new TesseractLanguage { Filename = "dzo.traineddata.zip", Code = "dzo", LangName = "Dzongkha", Size = 0.75, Sha1 = "2d5493a2157d1cf910b4fc8b3e0d861e25dd918a" },
            new TesseractLanguage { Filename = "ell.traineddata.zip", Code = "ell", LangName = "Greek", Size = 3.93, Sha1 = "0e3f029af86d83bbf1291bbc58d574829b4df053" },
            new TesseractLanguage { Filename = "eng.traineddata.zip", Code = "eng", LangName = "English", Size = 12.29, Sha1 = "64d9aa9654d5ee9e82d9b693c3445a91ffbd7b93" },
            new TesseractLanguage { Filename = "enm.traineddata.zip", Code = "enm", LangName = "English (Middle)", Size = 4.58, Sha1 = "06c7e1b3f4135290eae297aece61b11a413de4ba" },
            new TesseractLanguage { Filename = "epo.traineddata.zip", Code = "epo", LangName = "Esperanto", Size = 6.50, Sha1 = "7e4b1cc89c5fcbd9f2b2ae8caad09aa025452f1f" },
            new TesseractLanguage { Filename = "equ.traineddata.zip", Code = "equ", LangName = "Math / equation detection", Size = 0.79, Sha1 = "b15b9a1c006cebac5ffc35569fe01b3e7ee53e72" },
            new TesseractLanguage { Filename = "est.traineddata.zip", Code = "est", LangName = "Estonian", Size = 8.49, Sha1 = "6aefab9f0bdc0c080ee681d3fd359ecef3cd9269" },
            new TesseractLanguage { Filename = "eus.traineddata.zip", Code = "eus", LangName = "Basque", Size = 6.07, Sha1 = "c12b5e15c5bd89e11029c071050d093b2ba188c5" },
            new TesseractLanguage { Filename = "fao.traineddata.zip", Code = "fao", LangName = "Faroese", Size = 3.69, Sha1 = "a7da4c8c4c299557a655e0e5c1045e5beace9c9b" },
            new TesseractLanguage { Filename = "fas.traineddata.zip", Code = "fas", LangName = "Persian", Size = 0.71, Sha1 = "a3aadf776fc9248444d68c971066cf3f86d3c4ce", RTL = true },
            new TesseractLanguage { Filename = "fil.traineddata.zip", Code = "fil", LangName = "Filipino", Size = 2.31, Sha1 = "513edccb087eed2771f67c355d4c4938c8811e75" },
            new TesseractLanguage { Filename = "fin.traineddata.zip", Code = "fin", LangName = "Finnish", Size = 12.19, Sha1 = "45db275878b1e73777b5525dc2f01c8f1d3115fa" },
            new TesseractLanguage { Filename = "fra.traineddata.zip", Code = "fra", LangName = "French", Size = 6.55, Sha1 = "0ac9eeb04b334ef29c6a63e86c82aa91d0fe6fce" },
            new TesseractLanguage { Filename = "frk.traineddata.zip", Code = "frk", LangName = "Frankish", Size = 13.08, Sha1 = "2a22d40a403a4e03017de8ae97d4800dbcf21e06" },
            new TesseractLanguage { Filename = "frm.traineddata.zip", Code = "frm", LangName = "French (Middle)", Size = 8.30, Sha1 = "9ad7ab932c2b140e2dc665121ae4ab54e6df0026" },
            new TesseractLanguage { Filename = "fry.traineddata.zip", Code = "fry", LangName = "Frisian (Western)", Size = 2.42, Sha1 = "a455b00abd59502f6f6be2cddd11ddded3c6302e" },
            new TesseractLanguage { Filename = "gla.traineddata.zip", Code = "gla", LangName = "Gaelic", Size = 3.33, Sha1 = "f1f002acf9bb3d17b97e044ddfd654211364129c" },
            new TesseractLanguage { Filename = "gle.traineddata.zip", Code = "gle", LangName = "Irish", Size = 2.55, Sha1 = "fd95035f971a61472be035b977f91865275d2b4f" },
            new TesseractLanguage { Filename = "glg.traineddata.zip", Code = "glg", LangName = "Galician", Size = 5.38, Sha1 = "ab121f2c06eae328335eff6086f3da32549ac9f1" },
            new TesseractLanguage { Filename = "grc.traineddata.zip", Code = "grc", LangName = "Greek (Ancient)", Size = 3.98, Sha1 = "2aec27b8494b63be72e8b493184ee83e75399ffd" },
            new TesseractLanguage { Filename = "guj.traineddata.zip", Code = "guj", LangName = "Gujarati", Size = 1.88, Sha1 = "8365aa9722bb76774bb0648ac23233ed889464a1" },
            new TesseractLanguage { Filename = "hat.traineddata.zip", Code = "hat", LangName = "Haitian", Size = 3.40, Sha1 = "6b2eb7d203d7fd12ee4d283f3a096d2efa9eb1f1" },
            new TesseractLanguage { Filename = "heb.traineddata.zip", Code = "heb", LangName = "Hebrew", Size = 2.53, Sha1 = "8a083a920a85148966472b23f0fef57aa25d49d8", RTL = true },
            new TesseractLanguage { Filename = "hin.traineddata.zip", Code = "hin", LangName = "Hindi", Size = 2.22, Sha1 = "469b764d3af97d39fb175ae1ace182033a986706" },
            new TesseractLanguage { Filename = "hrv.traineddata.zip", Code = "hrv", LangName = "Croatian", Size = 7.23, Sha1 = "9d53c8d5c97ff8f40c2bd953a468a170d163ac77" },
            new TesseractLanguage { Filename = "hun.traineddata.zip", Code = "hun", LangName = "Hungarian", Size = 9.58, Sha1 = "adfc68325b8fb215b069e0f093ef96e68d0d068f" },
            new TesseractLanguage { Filename = "hye.traineddata.zip", Code = "hye", LangName = "Armenian", Size = 2.97, Sha1 = "a058b8ec58653ac3cad34823b4f8aec04fb970d9" },
            new TesseractLanguage { Filename = "iku.traineddata.zip", Code = "iku", LangName = "Inuktitut", Size = 3.10, Sha1 = "2a8927e92e3af0d45550ff8a2215310ea9b4bc35" },
            new TesseractLanguage { Filename = "ind.traineddata.zip", Code = "ind", LangName = "Indonesian", Size = 4.24, Sha1 = "d3c3b32e71d2fac63661cbcc842927d9a2825be8" },
            new TesseractLanguage { Filename = "isl.traineddata.zip", Code = "isl", LangName = "Icelandic", Size = 4.96, Sha1 = "9360af20b740d2313863dbe527fd831704bf2121" },
            new TesseractLanguage { Filename = "ita.traineddata.zip", Code = "ita", LangName = "Italian", Size = 7.80, Sha1 = "916998186f658546c3407201127c588539ab447c" },
            new TesseractLanguage { Filename = "ita_old.traineddata.zip", Code = "ita_old", LangName = "Italian (Old)", Size = 8.80, Sha1 = "b6a7efe00f7ce34f75b4a3a91c2c1d3ea83133ca" },
            new TesseractLanguage { Filename = "jav.traineddata.zip", Code = "jav", LangName = "Javanese", Size = 4.74, Sha1 = "5dd426e68a1a2ca4d6a6a771226e54a1a943a61e" },
            new TesseractLanguage { Filename = "jpn.traineddata.zip", Code = "jpn", LangName = "Japanese", Size = 16.77, Sha1 = "73b54f8cd99edffa20627583a82add77462f593a" },
            new TesseractLanguage { Filename = "jpn_vert.traineddata.zip", Code = "jpn_vert", LangName = "Japanese (Vertical)", Size = 3.88, Sha1 = "ff0e822c64c0ba88f9cd4caf6bc90187446da6f2" },
            new TesseractLanguage { Filename = "kan.traineddata.zip", Code = "kan", LangName = "Kannada", Size = 3.66, Sha1 = "7b6e48a0674c2adb39b1b8819751e7fdf1b54722" },
            new TesseractLanguage { Filename = "kat.traineddata.zip", Code = "kat", LangName = "Georgian", Size = 4.29, Sha1 = "c55fb40f2375c91d35409af690d8f139b92a3903" },
            new TesseractLanguage { Filename = "kat_old.traineddata.zip", Code = "kat_old", LangName = "Georgian (Old)", Size = 0.92, Sha1 = "4c94b5f3c90e8034536a7dc1f1e74ec66cfa1bb5" },
            new TesseractLanguage { Filename = "kaz.traineddata.zip", Code = "kaz", LangName = "Kazakh", Size = 5.70, Sha1 = "e07e7ffb3c656c15637b23e56cd95dc1c584a059" },
            new TesseractLanguage { Filename = "khm.traineddata.zip", Code = "khm", LangName = "Khmer (Central)", Size = 2.05, Sha1 = "94b300a9051018506026bfb58ce95da9bc0bd00a" },
            new TesseractLanguage { Filename = "kir.traineddata.zip", Code = "kir", LangName = "Kirghiz", Size = 10.46, Sha1 = "d3d8cc2168427f6dfe584349109bf18be05f6461" },
            new TesseractLanguage { Filename = "kor.traineddata.zip", Code = "kor", LangName = "Korean", Size = 7.66, Sha1 = "8e7dfdf16af0abd98ba87dbb5db59f140fd0429e" },
            new TesseractLanguage { Filename = "kor_vert.traineddata.zip", Code = "kor_vert", LangName = "Korean (Vertical)", Size = 1.18, Sha1 = "76349e042e19e5ed4bffcd4ed6f56159b2620536" },
            new TesseractLanguage { Filename = "kur.traineddata.zip", Code = "kur", LangName = "Kurdish", Size = 0.73, Sha1 = "3dd03488c9e05b6dcca8767c3b3d0d375a214723", RTL = true },
            new TesseractLanguage { Filename = "kur_ara.traineddata.zip", Code = "kur_ara", LangName = "Kurdish (Arabic)", Size = 1.83, Sha1 = "a3e0c096cda284b963dce271c358174449fea4dc" },
            new TesseractLanguage { Filename = "lao.traineddata.zip", Code = "lao", LangName = "Lao", Size = 6.52, Sha1 = "0c577c9b9b57a5312dc5cfe1ee3bbf5d728e5b50" },
            new TesseractLanguage { Filename = "lat.traineddata.zip", Code = "lat", LangName = "Latin", Size = 5.38, Sha1 = "5296894c777b799199ecbab99e5880e814fbab5e" },
            new TesseractLanguage { Filename = "lav.traineddata.zip", Code = "lav", LangName = "Latvian", Size = 5.32, Sha1 = "83b04ff7616468868bba6f7c9a7e071d79ddc90f" },
            new TesseractLanguage { Filename = "lit.traineddata.zip", Code = "lit", LangName = "Lithuanian", Size = 6.42, Sha1 = "9d244f95eceee451b54274159fddf84739ec7294" },
            new TesseractLanguage { Filename = "ltz.traineddata.zip", Code = "ltz", LangName = "Luxembourgish", Size = 3.49, Sha1 = "2f1ed3052e57dc7dbf548d1cdd96e14a696f882a" },
            new TesseractLanguage { Filename = "mal.traineddata.zip", Code = "mal", LangName = "Malayalam", Size = 4.73, Sha1 = "11616e5cf327229775b99eb48c71a9733cb18eac" },
            new TesseractLanguage { Filename = "mar.traineddata.zip", Code = "mar", LangName = "Marathi", Size = 2.84, Sha1 = "daa3124cd616bbbab1d1540d65fea8c943f824d5" },
            new TesseractLanguage { Filename = "mkd.traineddata.zip", Code = "mkd", LangName = "Macedonian", Size = 2.83, Sha1 = "acaf7cef9c12557db3a5ece01e2f84c67948bf9b" },
            new TesseractLanguage { Filename = "mlt.traineddata.zip", Code = "mlt", LangName = "Maltese", Size = 4.17, Sha1 = "5629bbe2c8bd96ef0a8fb0cc1fbd969c2bb59496" },
            new TesseractLanguage { Filename = "mon.traineddata.zip", Code = "mon", LangName = "Mongolian", Size = 2.53, Sha1 = "ad42b4564c70088b59802bfc562d04bc6a84ef71" },
            new TesseractLanguage { Filename = "mri.traineddata.zip", Code = "mri", LangName = "Maori", Size = 1.05, Sha1 = "b049cf217b38183855630823e3353259d5d1dd2c" },
            new TesseractLanguage { Filename = "msa.traineddata.zip", Code = "msa", LangName = "Malay", Size = 4.73, Sha1 = "c3f2017c05cc0d6b96f525430625a4b08db63c6e" },
            new TesseractLanguage { Filename = "mya.traineddata.zip", Code = "mya", LangName = "Burmese", Size = 5.15, Sha1 = "16fef298116a5c90e08e8d360322a75d0a394272" },
            new TesseractLanguage { Filename = "nep.traineddata.zip", Code = "nep", LangName = "Nepali", Size = 2.04, Sha1 = "54c5f5db4207ce9254a317cc250bf6eecbd9447d" },
            new TesseractLanguage { Filename = "nld.traineddata.zip", Code = "nld", LangName = "Dutch", Size = 12.59, Sha1 = "56d37209c62e9e6afa51d1e001886564fdd6c45e" },
            new TesseractLanguage { Filename = "nor.traineddata.zip", Code = "nor", LangName = "Norwegian", Size = 7.45, Sha1 = "f0466e0973265352dd37ac7d8a25ad6b76d0a0ee" },
            new TesseractLanguage { Filename = "oci.traineddata.zip", Code = "oci", LangName = "Occitan", Size = 6.09, Sha1 = "915f3df3502995e2867627c5bde58c83644ba796" },
            new TesseractLanguage { Filename = "ori.traineddata.zip", Code = "ori", LangName = "Oriya", Size = 2.04, Sha1 = "2da32dc862e1fc074185fd97f09c0a55edefaf93" },
//            new TesseractLanguage { Filename = "osd.traineddata.zip", Code = "osd", LangName = "", Size = 8.22, Sha1 = "8162903ddc718157e6feeabbfdafe0e375a38001" },
            new TesseractLanguage { Filename = "pan.traineddata.zip", Code = "pan", LangName = "Panjabi", Size = 1.66, Sha1 = "c29528e151531a9891904331f8e320d329a3dd92" },
            new TesseractLanguage { Filename = "pol.traineddata.zip", Code = "pol", LangName = "Polish", Size = 9.89, Sha1 = "8c8e6a3521e17c671defc04607808af97556d07b" },
            new TesseractLanguage { Filename = "por.traineddata.zip", Code = "por", LangName = "Portuguese", Size = 7.38, Sha1 = "58a8b3cddd0c0bf516bf82b7464378662d7e80f5" },
            new TesseractLanguage { Filename = "pus.traineddata.zip", Code = "pus", LangName = "Pushto", Size = 2.73, Sha1 = "83c093d6d2c821d6d9a3f734f753001b2590614a" },
            new TesseractLanguage { Filename = "que.traineddata.zip", Code = "que", LangName = "Quechua", Size = 4.93, Sha1 = "46ab85ef746d6cc0130a9ba5756fb56a250758e4" },
            new TesseractLanguage { Filename = "ron.traineddata.zip", Code = "ron", LangName = "Romanian", Size = 5.65, Sha1 = "d9e931572522802046750d5110ac7aa9d78c816c" },
            new TesseractLanguage { Filename = "rus.traineddata.zip", Code = "rus", LangName = "Russian", Size = 9.74, Sha1 = "949a12e51f29aa02dbcd7e1f41d547780876c335" },
            new TesseractLanguage { Filename = "san.traineddata.zip", Code = "san", LangName = "Sanskrit", Size = 10.86, Sha1 = "13129ccc5fd154f69e1632ed2bdfad3785d0f944" },
            new TesseractLanguage { Filename = "sin.traineddata.zip", Code = "sin", LangName = "Sinhala", Size = 2.19, Sha1 = "7d3a2c6208a4562db3e97a7e0313bd8b9cbf52a2" },
            new TesseractLanguage { Filename = "slk.traineddata.zip", Code = "slk", LangName = "Slovakian", Size = 7.50, Sha1 = "7cdbd545c966a281d0d8954187e1ee612b6f6d65" },
            new TesseractLanguage { Filename = "slk_frak.traineddata.zip", Code = "slk_frak", LangName = "Slovakian (Fraktur)", Size = 0.28, Sha1 = "050b6b8515e7e252b86a121207c205a574e9cd5b" },
            new TesseractLanguage { Filename = "slv.traineddata.zip", Code = "slv", LangName = "Slovenian", Size = 4.93, Sha1 = "0b20f99d0a755db2faffe1508940b166b06835af" },
            new TesseractLanguage { Filename = "snd.traineddata.zip", Code = "snd", LangName = "Sindhi", Size = 2.70, Sha1 = "afc0abcb26a75d833452f0d313f8d428dcfe2613" },
            new TesseractLanguage { Filename = "spa.traineddata.zip", Code = "spa", LangName = "Spanish", Size = 9.03, Sha1 = "21a32e0e3981bb0d62836327567361327173e0cc" },
            new TesseractLanguage { Filename = "spa_old.traineddata.zip", Code = "spa_old", LangName = "Spanish (Old)", Size = 9.79, Sha1 = "5d9e6c07d573f47e90443034e2b2505527315abb" },
            new TesseractLanguage { Filename = "sqi.traineddata.zip", Code = "sqi", LangName = "Albanian", Size = 4.13, Sha1 = "806024671905452d5c4844e15fcccb86863c5563" },
            new TesseractLanguage { Filename = "srp.traineddata.zip", Code = "srp", LangName = "Serbian", Size = 3.92, Sha1 = "30a4f7cc1ddff1154fe49d2a6e7a0edb747a9ec8" },
            new TesseractLanguage { Filename = "srp_latn.traineddata.zip", Code = "srp_latn", LangName = "Serbian (Latin)", Size = 5.65, Sha1 = "8d7f141429265ac927f1f05f4b7ae4b397a8c4ca" },
            new TesseractLanguage { Filename = "sun.traineddata.zip", Code = "sun", LangName = "Sundanese", Size = 1.46, Sha1 = "92643e9e815574d99a125406923bc96c1581bc41" },
            new TesseractLanguage { Filename = "swa.traineddata.zip", Code = "swa", LangName = "Swahili", Size = 3.52, Sha1 = "fc19a0dc5a7047d134e519cf5b0a7a5f2bbcb34e" },
            new TesseractLanguage { Filename = "swe.traineddata.zip", Code = "swe", LangName = "Swedish", Size = 8.42, Sha1 = "d30dbe87e640bd7e95265bd4372ccb0f83722baa" },
            new TesseractLanguage { Filename = "syr.traineddata.zip", Code = "syr", LangName = "Syriac", Size = 3.09, Sha1 = "2047b388123d3511e76a21441458725ec8922658" },
            new TesseractLanguage { Filename = "tam.traineddata.zip", Code = "tam", LangName = "Tamil", Size = 2.65, Sha1 = "8febcf0011ad2642d428cc02915190622e0d9381" },
            new TesseractLanguage { Filename = "tat.traineddata.zip", Code = "tat", LangName = "Tatar", Size = 1.74, Sha1 = "0aa474fdb1dcb8c6b634b366e85dcc4620c4c7fe" },
            new TesseractLanguage { Filename = "tel.traineddata.zip", Code = "tel", LangName = "Telugu", Size = 2.85, Sha1 = "3b0ee160a7af431a3eefabdba48b600db41b8148" },
            new TesseractLanguage { Filename = "tgk.traineddata.zip", Code = "tgk", LangName = "Tajik", Size = 2.62, Sha1 = "83c832eadbb937ef6bd707c07bc43bccc246accd" },
            new TesseractLanguage { Filename = "tgl.traineddata.zip", Code = "tgl", LangName = "Tagalog", Size = 3.13, Sha1 = "a0fdf7c7b935e33260aee265c20b96d0b90d5b08" },
            new TesseractLanguage { Filename = "tha.traineddata.zip", Code = "tha", LangName = "Thai", Size = 1.73, Sha1 = "1289ca3585658dbba7429621d8ab8833c872cafc" },
            new TesseractLanguage { Filename = "tir.traineddata.zip", Code = "tir", LangName = "Tigrinya", Size = 1.18, Sha1 = "5aacd48843a01270729954fac165e215345d1439" },
            new TesseractLanguage { Filename = "ton.traineddata.zip", Code = "ton", LangName = "Tonga (Tonga Islands)", Size = 1.13, Sha1 = "00a679fb18715dc2cb3bda6fa6ce682519f35f9d" },
            new TesseractLanguage { Filename = "tur.traineddata.zip", Code = "tur", LangName = "Turkish", Size = 9.58, Sha1 = "42993630cc2ca6e77743decce6db17c937d4d565" },
            new TesseractLanguage { Filename = "uig.traineddata.zip", Code = "uig", LangName = "Uighur", Size = 3.55, Sha1 = "b4d47e24f7f1f35db23450efa59d5aad20aab8a4" },
            new TesseractLanguage { Filename = "ukr.traineddata.zip", Code = "ukr", LangName = "Ukrainian", Size = 6.48, Sha1 = "fa30fb31bd68d252974fa0902b13a233c3860e49" },
            new TesseractLanguage { Filename = "urd.traineddata.zip", Code = "urd", LangName = "Urdu", Size = 1.97, Sha1 = "e3288ad91bef0987b97c2f465b1d5ad918bd8a01", RTL = true },
            new TesseractLanguage { Filename = "uzb.traineddata.zip", Code = "uzb", LangName = "Uzbek", Size = 7.48, Sha1 = "425c50636d22815508ec4d9c78b7218294151bcf" },
            new TesseractLanguage { Filename = "uzb_cyrl.traineddata.zip", Code = "uzb_cyrl", LangName = "Uzbek (Cyrillic)", Size = 2.78, Sha1 = "6a8ac1df9932528848c07b13be15526cea22d458" },
            new TesseractLanguage { Filename = "vie.traineddata.zip", Code = "vie", LangName = "Vietnamese", Size = 4.06, Sha1 = "f3d67cc479ae535393d6a544aae8752a718878c4" },
            new TesseractLanguage { Filename = "yid.traineddata.zip", Code = "yid", LangName = "Yiddish", Size = 2.38, Sha1 = "fbaf27e063c45fb366dc5cf38a472b616fc2553a" },
            new TesseractLanguage { Filename = "yor.traineddata.zip", Code = "yor", LangName = "Yoruba", Size = 1.14, Sha1 = "b7bcc0416531f0432af9ed523887d0aa0dfb272b" },
        };

        #endregion
    }
}
