using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2.Ocr
{
    public class OcrLanguageManager
    {
        public DirectoryInfo GetTessdataDir()
        {
            var dir = new DirectoryInfo(Path.Combine(Paths.Components, "tesseract-3.0.2", "tessdata"));
            if (!dir.Exists)
            {
                dir.Create();
            }
            return dir;
        }

        public IEnumerable<OcrLanguage> GetDownloadedLanguages()
        {
            var downloadedCodes = GetDownloadedCodes();
            return AllLanguages.Where(x => downloadedCodes.Contains(x.Code));
        }

        public IEnumerable<OcrLanguage> GetMissingLanguages()
        {
            var downloadedCodes = GetDownloadedCodes();
            return AllLanguages.Where(x => !downloadedCodes.Contains(x.Code));
        }

        private HashSet<string> GetDownloadedCodes()
        {
            var tessdataFolder = GetTessdataDir();
            if (!tessdataFolder.Exists)
            {
                return new HashSet<string>();
            }
            return new HashSet<string>(tessdataFolder.GetFiles("*.traineddata").Select(x => Path.GetFileNameWithoutExtension(x.Name)));
        }

        private static readonly OcrLanguage[] AllLanguages =
        {
            new OcrLanguage { Filename = "epo_alt.traineddata.gz", Code = "epo_alt", LangName = "Esperanto alternative", Size = 1.41 },
            new OcrLanguage { Filename = "eng.traineddata.gz", Code = "eng", LangName = "English", Size = 9.02 },
            new OcrLanguage { Filename = "ukr.traineddata.gz", Code = "ukr", LangName = "Ukrainian", Size = 0.89 },
            new OcrLanguage { Filename = "tur.traineddata.gz", Code = "tur", LangName = "Turkish", Size = 3.40 },
            new OcrLanguage { Filename = "tha.traineddata.gz", Code = "tha", LangName = "Thai", Size = 3.63 },
            new OcrLanguage { Filename = "tgl.traineddata.gz", Code = "tgl", LangName = "Tagalog", Size = 1.43 },
            new OcrLanguage { Filename = "tel.traineddata.gz", Code = "tel", LangName = "Telugu", Size = 5.53 },
            new OcrLanguage { Filename = "tam.traineddata.gz", Code = "tam", LangName = "Tamil", Size = 3.34 },
            new OcrLanguage { Filename = "swe.traineddata.gz", Code = "swe", LangName = "Swedish", Size = 2.34 },
            new OcrLanguage { Filename = "swa.traineddata.gz", Code = "swa", LangName = "Swahili", Size = 0.72 },
            new OcrLanguage { Filename = "srp.traineddata.gz", Code = "srp", LangName = "Serbian (Latin)", Size = 1.69 },
            new OcrLanguage { Filename = "sqi.traineddata.gz", Code = "sqi", LangName = "Albanian", Size = 1.59 },
            new OcrLanguage { Filename = "spa_old.traineddata.gz", Code = "spa_old", LangName = "Spanish (Old)", Size = 5.39 },
            new OcrLanguage { Filename = "spa.traineddata.gz", Code = "spa", LangName = "Spanish", Size = 0.84 },
            new OcrLanguage { Filename = "slv.traineddata.gz", Code = "slv", LangName = "Slovenian", Size = 1.54 },
            new OcrLanguage { Filename = "slk.traineddata.gz", Code = "slk", LangName = "Slovakian", Size = 2.11 },
            new OcrLanguage { Filename = "ron.traineddata.gz", Code = "ron", LangName = "Romanian", Size = 0.87 },
            new OcrLanguage { Filename = "por.traineddata.gz", Code = "por", LangName = "Portuguese", Size = 0.87 },
            new OcrLanguage { Filename = "pol.traineddata.gz", Code = "pol", LangName = "Polish", Size = 6.70 },
            new OcrLanguage { Filename = "nor.traineddata.gz", Code = "nor", LangName = "Norwegian", Size = 2.09 },
            new OcrLanguage { Filename = "nld.traineddata.gz", Code = "nld", LangName = "Dutch", Size = 1.08 },
            new OcrLanguage { Filename = "msa.traineddata.gz", Code = "msa", LangName = "Malay", Size = 1.59 },
            new OcrLanguage { Filename = "mlt.traineddata.gz", Code = "mlt", LangName = "Maltese", Size = 1.40 },
            new OcrLanguage { Filename = "mkd.traineddata.gz", Code = "mkd", LangName = "Macedonian", Size = 1.11 },
            new OcrLanguage { Filename = "mal.traineddata.gz", Code = "mal", LangName = "Malayalam", Size = 5.69 },
            new OcrLanguage { Filename = "lit.traineddata.gz", Code = "lit", LangName = "Lithuanian", Size = 1.70 },
            new OcrLanguage { Filename = "lav.traineddata.gz", Code = "lav", LangName = "Latvian", Size = 1.76 },
            new OcrLanguage { Filename = "kor.traineddata.gz", Code = "kor", LangName = "Korean", Size = 5.11 },
            new OcrLanguage { Filename = "kan.traineddata.gz", Code = "kan", LangName = "Kannada", Size = 4.19 },
            new OcrLanguage { Filename = "ita_old.traineddata.gz", Code = "ita_old", LangName = "Italian (Old)", Size = 3.28 },
            new OcrLanguage { Filename = "ita.traineddata.gz", Code = "ita", LangName = "Italian", Size = 0.90 },
            new OcrLanguage { Filename = "isl.traineddata.gz", Code = "isl", LangName = "Icelandic", Size = 1.56 },
            new OcrLanguage { Filename = "ind.traineddata.gz", Code = "ind", LangName = "Indonesian", Size = 1.79 },
            new OcrLanguage { Filename = "chr.traineddata.gz", Code = "chr", LangName = "Cherokee", Size = 0.31 },
            new OcrLanguage { Filename = "hun.traineddata.gz", Code = "hun", LangName = "Hungarian", Size = 2.93 },
            new OcrLanguage { Filename = "hrv.traineddata.gz", Code = "hrv", LangName = "Croatian", Size = 1.84 },
            new OcrLanguage { Filename = "hin.traineddata.gz", Code = "hin", LangName = "Hindi", Size = 6.28 },
            new OcrLanguage { Filename = "heb.traineddata.gz", Code = "heb", LangName = "Hebrew", Size = 1.00 },
            new OcrLanguage { Filename = "glg.traineddata.gz", Code = "glg", LangName = "Galician", Size = 1.60 },
            new OcrLanguage { Filename = "frm.traineddata.gz", Code = "frm", LangName = "Middle French (ca. 1400-1600)", Size = 4.91 },
            new OcrLanguage { Filename = "frk.traineddata.gz", Code = "frk", LangName = "Frankish", Size = 5.64 },
            new OcrLanguage { Filename = "fra.traineddata.gz", Code = "fra", LangName = "French", Size = 1.31 },
            new OcrLanguage { Filename = "fin.traineddata.gz", Code = "fin", LangName = "Finnish", Size = 0.93 },
            new OcrLanguage { Filename = "eus.traineddata.gz", Code = "eus", LangName = "Basque", Size = 1.57 },
            new OcrLanguage { Filename = "est.traineddata.gz", Code = "est", LangName = "Estonian", Size = 1.82 },
            new OcrLanguage { Filename = "equ.traineddata.gz", Code = "equ", LangName = "Math / equation detection", Size = 0.78 },
            new OcrLanguage { Filename = "epo.traineddata.gz", Code = "epo", LangName = "Esperanto", Size = 1.18 },
            new OcrLanguage { Filename = "enm.traineddata.gz", Code = "enm", LangName = "Middle English (1100-1500)", Size = 0.59 },
            new OcrLanguage { Filename = "ell.traineddata.gz", Code = "ell", LangName = "Greek", Size = 0.82 },
            new OcrLanguage { Filename = "deu.traineddata.gz", Code = "deu", LangName = "German", Size = 0.95 },
            new OcrLanguage { Filename = "dan.traineddata.gz", Code = "dan", LangName = "Danish", Size = 1.88 },
            new OcrLanguage { Filename = "ces.traineddata.gz", Code = "ces", LangName = "Czech", Size = 0.99 },
            new OcrLanguage { Filename = "cat.traineddata.gz", Code = "cat", LangName = "Catalan", Size = 1.58 },
            new OcrLanguage { Filename = "bul.traineddata.gz", Code = "bul", LangName = "Bulgarian", Size = 1.53 },
            new OcrLanguage { Filename = "ben.traineddata.gz", Code = "ben", LangName = "Bengali", Size = 6.46 },
            new OcrLanguage { Filename = "bel.traineddata.gz", Code = "bel", LangName = "Belarusian", Size = 1.22 },
            new OcrLanguage { Filename = "aze.traineddata.gz", Code = "aze", LangName = "Azerbaijani", Size = 1.36 },
            new OcrLanguage { Filename = "ara.traineddata.gz", Code = "ara", LangName = "Arabic", Size = 1.62 },
            new OcrLanguage { Filename = "afr.traineddata.gz", Code = "afr", LangName = "Afrikaans", Size = 1.03 },
            new OcrLanguage { Filename = "jpn.traineddata.gz", Code = "jpn", LangName = "Japanese", Size = 12.88 },
            new OcrLanguage { Filename = "chi_sim.traineddata.gz", Code = "chi_sim", LangName = "Chinese (Simplified)", Size = 16.89 },
            new OcrLanguage { Filename = "chi_tra.traineddata.gz", Code = "chi_tra", LangName = "Chinese (Traditional)", Size = 23.57 },
        };
    }
}
