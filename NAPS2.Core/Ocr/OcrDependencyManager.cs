using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrDependencyManager
    {
        public DirectoryInfo GetExecutableDir()
        {
            var dir = new DirectoryInfo(Path.Combine(Paths.Components, "tesseract-3.0.2"));
            if (!dir.Exists)
            {
                dir.Create();
            }
            return dir;
        }

        public string ExecutableFileName
        {
            get { return "tesseract.exe.gz"; }
        }

        public double ExecutableFileSize
        {
            get { return 1.02; }
        }

        public string ExecutableFileSha1
        {
            get { return "3a878249a8d49a0a4e83b2a9cb39e162aa8cf92e"; }
        }

        public DirectoryInfo GetLanguageDir()
        {
            var dir = new DirectoryInfo(Path.Combine(Paths.Components, "tesseract-3.0.2", "tessdata"));
            if (!dir.Exists)
            {
                dir.Create();
            }
            return dir;
        }

        public bool IsExecutableDownloaded
        {
            get { return new FileInfo(Path.Combine(GetExecutableDir().FullName, "tesseract.exe")).Exists; }
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
            var tessdataFolder = GetLanguageDir();
            if (!tessdataFolder.Exists)
            {
                return new HashSet<string>();
            }
            return new HashSet<string>(tessdataFolder.GetFiles("*.traineddata").Select(x => Path.GetFileNameWithoutExtension(x.Name)));
        }

        private static readonly OcrLanguage[] AllLanguages =
        {
            new OcrLanguage { Filename = "epo_alt.traineddata.gz", Code = "epo_alt", LangName = "Esperanto alternative", Size = 1.41, Sha1 = "04936fdca40b3823a15e76ea760cfc3f9abcd234" },
            new OcrLanguage { Filename = "eng.traineddata.gz", Code = "eng", LangName = "English", Size = 9.02, Sha1 = "fcff3bc4178cc32b0b3bcd7f8460a1dd267be37b" },
            new OcrLanguage { Filename = "ukr.traineddata.gz", Code = "ukr", LangName = "Ukrainian", Size = 0.89, Sha1 = "95edad899e4a3062da5882c9642f1d094a6d6049" },
            new OcrLanguage { Filename = "tur.traineddata.gz", Code = "tur", LangName = "Turkish", Size = 3.40, Sha1 = "da73464366cec8494653398ad250b98755d9ee38" },
            new OcrLanguage { Filename = "tha.traineddata.gz", Code = "tha", LangName = "Thai", Size = 3.63, Sha1 = "ee841bcd8dac76d9a73e53f98ae84a6130297e97" },
            new OcrLanguage { Filename = "tgl.traineddata.gz", Code = "tgl", LangName = "Tagalog", Size = 1.43, Sha1 = "01e534c94e6fa15f148efcfdc952e5a2fb9da743" },
            new OcrLanguage { Filename = "tel.traineddata.gz", Code = "tel", LangName = "Telugu", Size = 5.53, Sha1 = "ed70d9d8d31fd2107290bb3235b98cee4e84c838" },
            new OcrLanguage { Filename = "tam.traineddata.gz", Code = "tam", LangName = "Tamil", Size = 3.34, Sha1 = "68ca625d9fe67fed0a26cec4948d396bf22e5efe" },
            new OcrLanguage { Filename = "swe.traineddata.gz", Code = "swe", LangName = "Swedish", Size = 2.34, Sha1 = "cc849de19b2fa10230040a0c441e7a7cfc5ccb84" },
            new OcrLanguage { Filename = "swa.traineddata.gz", Code = "swa", LangName = "Swahili", Size = 0.72, Sha1 = "5755339ca2934ff8c55c43b1eccc2652d65003e7" },
            new OcrLanguage { Filename = "srp.traineddata.gz", Code = "srp", LangName = "Serbian (Latin)", Size = 1.69, Sha1 = "fa1fc8f3007a33f2eb9476e9dbba2d84b8113d94" },
            new OcrLanguage { Filename = "sqi.traineddata.gz", Code = "sqi", LangName = "Albanian", Size = 1.59, Sha1 = "f025f44a24b5a9163654bb3102532cfda81215b7" },
            new OcrLanguage { Filename = "spa_old.traineddata.gz", Code = "spa_old", LangName = "Spanish (Old)", Size = 5.39, Sha1 = "3b52c0b3cf3a667d5cdcb6df48ed2abcaea6c99d" },
            new OcrLanguage { Filename = "spa.traineddata.gz", Code = "spa", LangName = "Spanish", Size = 0.84, Sha1 = "1cc912b51268fe8bdd981210409cde871001461d" },
            new OcrLanguage { Filename = "slv.traineddata.gz", Code = "slv", LangName = "Slovenian", Size = 1.54, Sha1 = "2efc76388800fd85920e366c19329faa1d096b84" },
            new OcrLanguage { Filename = "slk.traineddata.gz", Code = "slk", LangName = "Slovakian", Size = 2.11, Sha1 = "86d883625d1a12f8215e2ad8c56bbd57270e5917" },
            new OcrLanguage { Filename = "ron.traineddata.gz", Code = "ron", LangName = "Romanian", Size = 0.87, Sha1 = "6c53b9a88c0037b723eb89f190531f21db2eab97" },
            new OcrLanguage { Filename = "por.traineddata.gz", Code = "por", LangName = "Portuguese", Size = 0.87, Sha1 = "9455cf0134b71ec26ef2e86736a30acca15ebe37" },
            new OcrLanguage { Filename = "pol.traineddata.gz", Code = "pol", LangName = "Polish", Size = 6.70, Sha1 = "c35a3a36610311052b31ab79c3dc3e717ebb5b3d" },
            new OcrLanguage { Filename = "nor.traineddata.gz", Code = "nor", LangName = "Norwegian", Size = 2.09, Sha1 = "7c0cae29f3875d545c6c89314821b897e909f646" },
            new OcrLanguage { Filename = "nld.traineddata.gz", Code = "nld", LangName = "Dutch", Size = 1.08, Sha1 = "025a8560383337cf09f387f8f15d4b28331dbba1" },
            new OcrLanguage { Filename = "msa.traineddata.gz", Code = "msa", LangName = "Malay", Size = 1.59, Sha1 = "2dea17d3ca47f7b9aab9289e832027edaf3d0bed" },
            new OcrLanguage { Filename = "mlt.traineddata.gz", Code = "mlt", LangName = "Maltese", Size = 1.40, Sha1 = "e36e500a812d1c6326b06c28e3c1514a3c124a9c" },
            new OcrLanguage { Filename = "mkd.traineddata.gz", Code = "mkd", LangName = "Macedonian", Size = 1.11, Sha1 = "563958f97fe639803d3f09f1be2df4a8db02d7fc" },
            new OcrLanguage { Filename = "mal.traineddata.gz", Code = "mal", LangName = "Malayalam", Size = 5.69, Sha1 = "15bdb9f88f6a9748194170601c6ceab6dc1651a9" },
            new OcrLanguage { Filename = "lit.traineddata.gz", Code = "lit", LangName = "Lithuanian", Size = 1.70, Sha1 = "f6807d62c06270cc564b386d669d7a949fcb90b1" },
            new OcrLanguage { Filename = "lav.traineddata.gz", Code = "lav", LangName = "Latvian", Size = 1.76, Sha1 = "6f957027be70160d3eb2959a679a01bd70cb3960" },
            new OcrLanguage { Filename = "kor.traineddata.gz", Code = "kor", LangName = "Korean", Size = 5.11, Sha1 = "022465161690dd4741ce169a9198d23d04d08829" },
            new OcrLanguage { Filename = "kan.traineddata.gz", Code = "kan", LangName = "Kannada", Size = 4.19, Sha1 = "e1e96bc15672006edb7eadd566d05d241c3b6eee" },
            new OcrLanguage { Filename = "ita_old.traineddata.gz", Code = "ita_old", LangName = "Italian (Old)", Size = 3.28, Sha1 = "53611181e481edfd71ff021d87fd8a373e5233d6" },
            new OcrLanguage { Filename = "ita.traineddata.gz", Code = "ita", LangName = "Italian", Size = 0.90, Sha1 = "b0339a5d3220f51dc1f7681b360f8e604ac32fbf" },
            new OcrLanguage { Filename = "isl.traineddata.gz", Code = "isl", LangName = "Icelandic", Size = 1.56, Sha1 = "4a4467a9976ceccf2cc6985d22cf25c5af8784e6" },
            new OcrLanguage { Filename = "ind.traineddata.gz", Code = "ind", LangName = "Indonesian", Size = 1.79, Sha1 = "65888925848d34141c2f74121ceae88621780fca" },
            new OcrLanguage { Filename = "chr.traineddata.gz", Code = "chr", LangName = "Cherokee", Size = 0.31, Sha1 = "26ed04712fb9d9a0495681abd89353a55943619e" },
            new OcrLanguage { Filename = "hun.traineddata.gz", Code = "hun", LangName = "Hungarian", Size = 2.93, Sha1 = "22035e1a751ea3c92f18f6c8730bf2e0dfbeaf67" },
            new OcrLanguage { Filename = "hrv.traineddata.gz", Code = "hrv", LangName = "Croatian", Size = 1.84, Sha1 = "09d2268f830bcfaf455e2480c03155e25ccecb29" },
            new OcrLanguage { Filename = "hin.traineddata.gz", Code = "hin", LangName = "Hindi", Size = 6.28, Sha1 = "b7141ec8c5a6e1d9ec0ebe05f6223c7fb1ff5e6f" },
            new OcrLanguage { Filename = "heb.traineddata.gz", Code = "heb", LangName = "Hebrew", Size = 1.00, Sha1 = "067f7a5f765151a69752b427e9d5041370c78fa0" },
            new OcrLanguage { Filename = "glg.traineddata.gz", Code = "glg", LangName = "Galician", Size = 1.60, Sha1 = "6d2e4426cb736cbefc5a2e21030a68d52f8ab159" },
            new OcrLanguage { Filename = "frm.traineddata.gz", Code = "frm", LangName = "Middle French (ca. 1400-1600)", Size = 4.91, Sha1 = "de26bfb24a1dd40b6cd636afee05a26ab4b95863" },
            new OcrLanguage { Filename = "frk.traineddata.gz", Code = "frk", LangName = "Frankish", Size = 5.64, Sha1 = "43169cd9ea6f97badac0b85c7f84fbcdaad01ef1" },
            new OcrLanguage { Filename = "fra.traineddata.gz", Code = "fra", LangName = "French", Size = 1.31, Sha1 = "37eb41185980bd3cefcac91deb5d82df76e93607" },
            new OcrLanguage { Filename = "fin.traineddata.gz", Code = "fin", LangName = "Finnish", Size = 0.93, Sha1 = "a15f44f5d908995e34f6cb53e59aea9cb60934cf" },
            new OcrLanguage { Filename = "eus.traineddata.gz", Code = "eus", LangName = "Basque", Size = 1.57, Sha1 = "459e6657d9fa72e9742cd380cba19c098c35fb2e" },
            new OcrLanguage { Filename = "est.traineddata.gz", Code = "est", LangName = "Estonian", Size = 1.82, Sha1 = "ead76e532e85dd64b6bba2c9bffe0c5cdb5c2ae8" },
            new OcrLanguage { Filename = "equ.traineddata.gz", Code = "equ", LangName = "Math / equation detection", Size = 0.78, Sha1 = "4be9562d0b7f055101b2c13d3e409fb9ca63b636" },
            new OcrLanguage { Filename = "epo.traineddata.gz", Code = "epo", LangName = "Esperanto", Size = 1.18, Sha1 = "402994dfb5863ae0b8ad627a95715931e88160fd" },
            new OcrLanguage { Filename = "enm.traineddata.gz", Code = "enm", LangName = "Middle English (1100-1500)", Size = 0.59, Sha1 = "c4dc13e109bff8dec1d0155b7e6e6cdbcc1e60d3" },
            new OcrLanguage { Filename = "ell.traineddata.gz", Code = "ell", LangName = "Greek", Size = 0.82, Sha1 = "f71c7c81b62ea65365504d29d3c2612d242b110b" },
            new OcrLanguage { Filename = "deu.traineddata.gz", Code = "deu", LangName = "German", Size = 0.95, Sha1 = "223d7104cb448c007d91ace9fd70f52ffb17ca07" },
            new OcrLanguage { Filename = "dan.traineddata.gz", Code = "dan", LangName = "Danish", Size = 1.88, Sha1 = "2193148e4535ab68e53dd34c26fa20f91b569df0" },
            new OcrLanguage { Filename = "ces.traineddata.gz", Code = "ces", LangName = "Czech", Size = 0.99, Sha1 = "efd5886d9ae461ed659f40fbe89315eea5210570" },
            new OcrLanguage { Filename = "cat.traineddata.gz", Code = "cat", LangName = "Catalan", Size = 1.58, Sha1 = "6dca4bec4e06745e2deb8c29d00cb565cafac0f7" },
            new OcrLanguage { Filename = "bul.traineddata.gz", Code = "bul", LangName = "Bulgarian", Size = 1.53, Sha1 = "4e7c6a9338ba03db38214f0c4450b8f7a4bdf391" },
            new OcrLanguage { Filename = "ben.traineddata.gz", Code = "ben", LangName = "Bengali", Size = 6.46, Sha1 = "1ff4d9bb8ee4b0374c7e2b2a46e09dc26f9fced3" },
            new OcrLanguage { Filename = "bel.traineddata.gz", Code = "bel", LangName = "Belarusian", Size = 1.22, Sha1 = "fb229dca0758c011b99bc6a2c3ff1cb84c639e05" },
            new OcrLanguage { Filename = "aze.traineddata.gz", Code = "aze", LangName = "Azerbaijani", Size = 1.36, Sha1 = "af71189c6c3711cc77e677e1004654aef568c8f2" },
            new OcrLanguage { Filename = "ara.traineddata.gz", Code = "ara", LangName = "Arabic", Size = 1.62, Sha1 = "69f398cb9b118a9d066353eaf3548c87670a008c" },
            new OcrLanguage { Filename = "afr.traineddata.gz", Code = "afr", LangName = "Afrikaans", Size = 1.03, Sha1 = "f7e3b67c91a05ca655d2a5f72ff72b11d19a3ac0" },
            new OcrLanguage { Filename = "jpn.traineddata.gz", Code = "jpn", LangName = "Japanese", Size = 12.88, Sha1 = "ec1608a652ae9f47b9af54199da310e59df35011" },
            new OcrLanguage { Filename = "chi_sim.traineddata.gz", Code = "chi_sim", LangName = "Chinese (Simplified)", Size = 16.89, Sha1 = "e22bd4006a0b7246a1d137b0417f9270b5ba4ca4" },
            new OcrLanguage { Filename = "chi_tra.traineddata.gz", Code = "chi_tra", LangName = "Chinese (Traditional)", Size = 23.57, Sha1 = "bf60fb7815e6f508f063f323ed3a3445116ffac4" },
            new OcrLanguage { Filename = "grc.traineddata.gz", Code = "grc", LangName = "Ancient Greek", Size = 3.22, Sha1 = "44a61a7f8a983995b6cb347c88a301a61cc5ef58" },
            new OcrLanguage { Filename = "rus.traineddata.gz", Code = "rus", LangName = "Russian", Size = 5.69, Sha1 = "7ef1432cf05ddcd4dbae5b0094a3448207027c57" },
            new OcrLanguage { Filename = "vie.traineddata.gz", Code = "vie", LangName = "Vietnamese", Size = 2.09, Sha1 = "184b6e56993d4b130a6c5b4c7e7fef990043d71c" },
        };
    }
}
