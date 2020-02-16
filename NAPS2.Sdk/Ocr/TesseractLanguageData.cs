using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Ocr
{
    public class TesseractLanguageData
    {
        public class TesseractLanguage
        {
            public TesseractLanguage(string filename, string code, string langName, double size, string sha1, bool rtl = false)
            {
                Filename = filename;
                Code = code;
                LangName = langName;
                Size = size;
                Sha1 = sha1;
                RTL = rtl;
            }
            
            public string Filename { get; }

            public string Code { get; }

            public string LangName { get; }

            public double Size { get; }

            public string Sha1 { get; }

            public bool RTL { get; }
        }

        protected TesseractLanguageData(TesseractLanguage[] data)
        {
            Data = data;
            LanguageMap = data.ToDictionary(x => $"ocr-{x.Code}", x => new Language(x.Code, x.LangName, x.RTL));
        }

        public Dictionary<string, Language> LanguageMap { get; set; }

        public TesseractLanguage[] Data { get; set; }

        #region Tesseract 3.04 Language Data (auto-generated)

        public static readonly TesseractLanguageData V304 = new TesseractLanguageData(new []
        {
            new TesseractLanguage("afr.traineddata.gz", "afr", "Afrikaans", 1.93, "a669186130bf1fc6c78226ac868c82b70a44c70b"),
            new TesseractLanguage("amh.traineddata.gz", "amh", "Amharic", 1.03, "1153cbbac7306d42e72ca639ff3f36f45dcb15a2"),
            new TesseractLanguage("ara.traineddata.gz", "ara", "Arabic", 1.62, "87b76c73fdcc4c54ec1f03d83b6df665430c2b06", true),
            new TesseractLanguage("asm.traineddata.gz", "asm", "Assamese", 6.56, "223900790d10f638b7dca2a8b8e8a15295d1f19c"),
            new TesseractLanguage("aze.traineddata.gz", "aze", "Azerbaijani", 2.54, "01607e49fe6ba6604f65d9b57c77b403ab74040a", true),
            new TesseractLanguage("aze_cyrl.traineddata.gz", "aze_cyrl", "Azerbaijani (Cyrillic)", 0.97, "f9c9b153e8825bb92d9c8005342ac3d5ea81d0bc"),
            new TesseractLanguage("bel.traineddata.gz", "bel", "Belarusian", 2.43, "3ac0935dd22f4f2730286d5cb127324d27718410"),
            new TesseractLanguage("ben.traineddata.gz", "ben", "Bengali", 6.45, "479674b283db6e84fdfb17386056f2e9a5b41b9c"),
            new TesseractLanguage("bod.traineddata.gz", "bod", "Tibetan", 10.74, "3ff199544dc9e7994658231cbc999878e23463db"),
            new TesseractLanguage("bos.traineddata.gz", "bos", "Bosnian", 1.87, "9d0bb89c53251789bba06de1452cf1a74d978f35"),
            new TesseractLanguage("bul.traineddata.gz", "bul", "Bulgarian", 2.20, "ac0481cc1fe62c3af5a34d57fa1571dfd2a95865"),
            new TesseractLanguage("cat.traineddata.gz", "cat", "Catalan", 1.97, "e1e1dc2e37f6b085bdefdb9d0d63d3ad086ef1f4"),
            new TesseractLanguage("ceb.traineddata.gz", "ceb", "Cebuano", 0.58, "f867102f828b6495996370eea6ed8688af219b17"),
            new TesseractLanguage("ces.traineddata.gz", "ces", "Czech", 4.65, "155f60a0994f1590d3d3ba29ec1a5bca3f16efdd"),
            new TesseractLanguage("chi_sim.traineddata.gz", "chi_sim", "Chinese (Simplified)", 17.60, "9bd65dcecd2581e8f588cec11cd1e2f754885fcb"),
            new TesseractLanguage("chi_tra.traineddata.gz", "chi_tra", "Chinese (Traditional)", 24.11, "5abef9af8a4fd83a0d156ee2e1d5234c80bb836b"),
            new TesseractLanguage("chr.traineddata.gz", "chr", "Cherokee", 0.36, "d3677cb6c57ec1b14625a5594dad159a1ad9ec93"),
            new TesseractLanguage("cym.traineddata.gz", "cym", "Welsh", 1.36, "a5d5733d45710f6da1c4b19f0903bf5edb10a484"),
            new TesseractLanguage("dan.traineddata.gz", "dan", "Danish", 2.76, "eb813b0c299261b9535a2c684e51f159f05ae8ea"),
            new TesseractLanguage("dan_frak.traineddata.gz", "dan_frak", "Danish (Fraktur)", 0.65, "dcb540024688da096399e52ff9826aad1d71479c"),
            new TesseractLanguage("deu.traineddata.gz", "deu", "German", 5.48, "f575f3fcb554077b906aaaac8850d5bd56967cbd"),
            new TesseractLanguage("deu_frak.traineddata.gz", "deu_frak", "German (Fraktur)", 0.78, "28ac257129f881b3a09c099004048bf6de4bc952"),
            new TesseractLanguage("dzo.traineddata.gz", "dzo", "Dzongkha", 1.32, "6eb0c943242e4d906cbebec2cf43b2ca63979424"),
            new TesseractLanguage("ell.traineddata.gz", "ell", "Greek", 2.00, "e54ab7455c1d4715652253321f693e221b61ac8b"),
            new TesseractLanguage("eng.traineddata.gz", "eng", "English", 9.02, "36bfd5953540b3c294c62402e303f381cee156f3"),
            new TesseractLanguage("enm.traineddata.gz", "enm", "Middle English (1100-1500)", 0.77, "02486b802f4f83b5d9198309955cbf4aa38e5e05"),
            new TesseractLanguage("epo.traineddata.gz", "epo", "Esperanto", 2.42, "465dfb934eb45116ebe7f3c4e3adf28826e49dca"),
            new TesseractLanguage("equ.traineddata.gz", "equ", "Math / equation detection", 0.78, "c9bc582875cf7c7903b529a9cdb0b9f4669b840d"),
            new TesseractLanguage("est.traineddata.gz", "est", "Estonian", 3.62, "d743f2456fa32ce7bbbb80cb40951eb742692596"),
            new TesseractLanguage("eus.traineddata.gz", "eus", "Basque", 1.83, "d991552b861e5ea1dca59ffca7e295b323e62bbf"),
            new TesseractLanguage("fas.traineddata.gz", "fas", "Persian", 1.75, "c8a7a6b11c3f455b07a397af2e51705a68ff5f77", true),
            new TesseractLanguage("fin.traineddata.gz", "fin", "Finnish", 4.98, "90232ad3572901a35bd4bbc736d47184171fa0fd"),
            new TesseractLanguage("fra.traineddata.gz", "fra", "French", 5.65, "2bebc5a4c981443c1cbff254e0ca3120004a6c7b"),
            new TesseractLanguage("frk.traineddata.gz", "frk", "Frankish", 6.64, "1a6984f8b5768ae663f293ea04594fca229bdb16"),
            new TesseractLanguage("frm.traineddata.gz", "frm", "Middle French (ca. 1400-1600)", 6.34, "64e0c6e00352833b206f8b26b6410d0d544b798d"),
            new TesseractLanguage("gle.traineddata.gz", "gle", "Irish", 1.25, "994c111e9c24e74bf7105f42a3e39d87ea24f258"),
            new TesseractLanguage("glg.traineddata.gz", "glg", "Galician", 2.04, "201c627e518099c15dbbecd72e6e4782e389f619"),
            new TesseractLanguage("grc.traineddata.gz", "grc", "Ancient Greek", 1.88, "ae58a943620c485d33ba95b3fcaca79314105d56"),
            new TesseractLanguage("guj.traineddata.gz", "guj", "Gujarati", 4.39, "f469d7257f39dcdd0668d768886f19084816b10e"),
            new TesseractLanguage("hat.traineddata.gz", "hat", "Haitian", 0.49, "1667e25ebfe6dc74695af413f291e20f1eec552a"),
            new TesseractLanguage("heb.traineddata.gz", "heb", "Hebrew", 1.51, "64401c999ef08d6190a11a4347c8f9acf40a8e50", true),
            new TesseractLanguage("hin.traineddata.gz", "hin", "Hindi", 6.28, "dae6a9a729ad84eded87fef69004d89249170d44"),
            new TesseractLanguage("hrv.traineddata.gz", "hrv", "Croatian", 3.33, "b05db705553607afe3d3f2385dc7f272f348a59c"),
            new TesseractLanguage("hun.traineddata.gz", "hun", "Hungarian", 4.62, "250f8b5ad6464e3f0ad8694c0b54392cf6c9d73b"),
            new TesseractLanguage("iku.traineddata.gz", "iku", "Inuktitut", 0.30, "119af8b174547aa9cb00f04512d4960d523863ad"),
            new TesseractLanguage("ind.traineddata.gz", "ind", "Indonesian", 2.51, "f46f56473ba850408499678c349bdb6dc544dc67"),
            new TesseractLanguage("isl.traineddata.gz", "isl", "Icelandic", 2.28, "54004c851361c36ddf48b4443caf79188fa757b6"),
            new TesseractLanguage("ita.traineddata.gz", "ita", "Italian", 5.40, "1730f0e32cad3bd76a4f58de67d7c8e2cde17b51"),
            new TesseractLanguage("ita_old.traineddata.gz", "ita_old", "Italian (Old)", 5.35, "b7a4293b464cbcce08fd5dc15a9831cff888cdf0"),
            new TesseractLanguage("jav.traineddata.gz", "jav", "Javanese", 1.60, "3caa600f063705a2649be289038f381ecdaa8989"),
            new TesseractLanguage("jpn.traineddata.gz", "jpn", "Japanese", 13.65, "7545927e6c60888a61556af4247e81c7a08cc17d"),
            new TesseractLanguage("kan.traineddata.gz", "kan", "Kannada", 15.12, "53d26da4fde19b5663f4e7748809ba4baf12fe96"),
            new TesseractLanguage("kat.traineddata.gz", "kat", "Georgian", 2.23, "8c48267883781ad2278f052259fe4094c64ef9bb"),
            new TesseractLanguage("kat_old.traineddata.gz", "kat_old", "Georgian (Old)", 0.19, "88e8312c3fc30ba03811d5d571e44158bc0ab5bf"),
            new TesseractLanguage("kaz.traineddata.gz", "kaz", "Kazakh", 1.65, "45c6603afcfe4d81990439df3bed13dd1b4c654b"),
            new TesseractLanguage("khm.traineddata.gz", "khm", "Central Khmer", 20.96, "d5a542959114b154db4db61419cd57aba1e3cf5a"),
            new TesseractLanguage("kir.traineddata.gz", "kir", "Kirghiz", 2.02, "ee9ba20cde7597688140fc43b14e49417d1052b7"),
            new TesseractLanguage("kor.traineddata.gz", "kor", "Korean", 5.11, "39b452ede31b196c66442ea580b5664377eabdab"),
            new TesseractLanguage("kur.traineddata.gz", "kur", "Kurdish", 0.73, "a36683c3f62415e1d12529b7642b9463c880db0c", true),
            new TesseractLanguage("lao.traineddata.gz", "lao", "Lao", 8.70, "95dbad397571d2d2c13ed63ddc16a51fca343cfb"),
            new TesseractLanguage("lat.traineddata.gz", "lat", "Latin", 2.04, "43dc27088ecce88915f6de15c7f6ec9037eebfee"),
            new TesseractLanguage("lav.traineddata.gz", "lav", "Latvian", 2.91, "db4e13d875a4c88bd6d8873a7db95fcbd7f9114b"),
            new TesseractLanguage("lit.traineddata.gz", "lit", "Lithuanian", 3.28, "fae20b8933a2c49fb9d98539299c7452d530514a"),
            new TesseractLanguage("mal.traineddata.gz", "mal", "Malayalam", 3.49, "77a6553e0a37ddf5935a4e81b918850b8babb379"),
            new TesseractLanguage("mar.traineddata.gz", "mar", "Marathi", 5.85, "36297ba7adad4e476815a1ab962b556994e85196"),
            new TesseractLanguage("mkd.traineddata.gz", "mkd", "Macedonian", 1.36, "63a9ce25d9e2ce9e169ac17e422564809be21fb2"),
            new TesseractLanguage("mlt.traineddata.gz", "mlt", "Maltese", 1.96, "18cb93ee612c4c7989c005cdf3a228c4e524db67"),
            new TesseractLanguage("msa.traineddata.gz", "msa", "Malay", 2.47, "a40a2af1a06db7cbf4ecef903bff645d7ee3cfc3"),
            new TesseractLanguage("mya.traineddata.gz", "mya", "Burmese", 29.36, "f5875d22dc164da4176856ced8521790dfa986a8"),
            new TesseractLanguage("nep.traineddata.gz", "nep", "Nepali", 6.53, "55940992c6269123a49c0f0f616d766f9cb3aa4c"),
            new TesseractLanguage("nld.traineddata.gz", "nld", "Dutch", 6.83, "7a19402e128c97ffb5044780c055344e4b92cceb"),
            new TesseractLanguage("nor.traineddata.gz", "nor", "Norwegian", 3.14, "33fd288a93a5260954b0fca37894ce50d8872971"),
            new TesseractLanguage("ori.traineddata.gz", "ori", "Oriya", 3.06, "cc4951bf162f3e06f83a7f63868dc0ba2a86c83c"),
//            new TesseractLanguage { Filename = "osd.traineddata.gz", Code = "osd", LangName = "", Size = 4.08, Sha1 = "d8c10c1fca9b954ca2500e6abeee94b50329f486" },
            new TesseractLanguage("pan.traineddata.gz", "pan", "Panjabi", 4.06, "ec846c1a93576f85878de4b06fa82241782cf2a4"),
            new TesseractLanguage("pol.traineddata.gz", "pol", "Polish", 5.41, "55a31b8724722219ce80f0a75685f267ae221d3d"),
            new TesseractLanguage("por.traineddata.gz", "por", "Portuguese", 5.06, "c486d3ba8ad2d7555f894352313f4c5cfb287dca"),
            new TesseractLanguage("pus.traineddata.gz", "pus", "Pushto", 0.88, "c45f471412ae0a7b4ed92141c828963911fa5f15"),
            new TesseractLanguage("ron.traineddata.gz", "ron", "Romanian", 2.99, "e21ef667ff7bb90904cf0d731ebe184854cde616"),
            new TesseractLanguage("rus.traineddata.gz", "rus", "Russian", 6.05, "96d7897ddecc7f944b5c1751e9ff44416cc3ee21"),
            new TesseractLanguage("san.traineddata.gz", "san", "Sanskrit", 9.52, "c324b96fc4f1dcd2295329081f18be98e1c71053"),
            new TesseractLanguage("sin.traineddata.gz", "sin", "Sinhala", 2.60, "145f8b7da56fe12340d4a0ce3f0c1385e437398c"),
            new TesseractLanguage("slk.traineddata.gz", "slk", "Slovakian", 3.45, "abe9737fb49c9284a10cbb87b9efa773234af5c3"),
            new TesseractLanguage("slk_frak.traineddata.gz", "slk_frak", "Slovakian (Fraktur)", 0.28, "e12b4fd2b4d2739656ed28142ba5db081d49fce2"),
            new TesseractLanguage("slv.traineddata.gz", "slv", "Slovenian", 2.47, "d94468d01fec2bbcb8be23e97ec5329ef58c541f"),
            new TesseractLanguage("spa.traineddata.gz", "spa", "Spanish", 6.31, "89160dbb92dbb5bcd6c48237315f6aa892450ef1"),
            new TesseractLanguage("spa_old.traineddata.gz", "spa_old", "Spanish (Old)", 6.57, "9d13656da6a91ca4717f9235340f0304c7f77110"),
            new TesseractLanguage("sqi.traineddata.gz", "sqi", "Albanian", 2.40, "30957e11c55610634dfdd2704ff0d6036c2e4ca5"),
            new TesseractLanguage("srp.traineddata.gz", "srp", "Serbian", 1.56, "5a7ef0c3c37d7f1891bde5a96b92b2fd3e48783a"),
            new TesseractLanguage("srp_latn.traineddata.gz", "srp_latn", "Serbian (Latin)", 2.27, "2aa8ff0e22440d3aab1a59e47b416bcd7ab2e7ae"),
            new TesseractLanguage("swa.traineddata.gz", "swa", "Swahili", 1.43, "6010b9255c1cd98c8bda39cd18904bf7782942e1"),
            new TesseractLanguage("swe.traineddata.gz", "swe", "Swedish", 3.64, "1bd6fd11f36b3ca04342a521773179269c5410e3"),
            new TesseractLanguage("syr.traineddata.gz", "syr", "Syriac", 1.06, "01aa53fd62897bcbfc053401405485d6f6aa9df9"),
            new TesseractLanguage("tam.traineddata.gz", "tam", "Tamil", 1.99, "eaca5e8c91d7995894ff2dafc4b824f305d6fff0"),
            new TesseractLanguage("tel.traineddata.gz", "tel", "Telugu", 16.81, "1f5b1e2f3d8a772b406e4a2b9d8ec38f1eec4cc6"),
            new TesseractLanguage("tgk.traineddata.gz", "tgk", "Tajik", 0.40, "b839d70a88e1dc2a019d1b7e76b83e5dcb0df440"),
            new TesseractLanguage("tgl.traineddata.gz", "tgl", "Tagalog", 1.56, "0bdbb9e5f763ebfeef8fc9cd0ba1913bd7309755"),
            new TesseractLanguage("tha.traineddata.gz", "tha", "Thai", 5.61, "7a171182716c99c19c1cc9b934a70ef5bee7893a"),
            new TesseractLanguage("tir.traineddata.gz", "tir", "Tigrinya", 0.60, "4292700b180a505c4a45666a13eac6e144b48615"),
            new TesseractLanguage("tur.traineddata.gz", "tur", "Turkish", 5.61, "8d72dc5ec5f22073f6b3ae2f79534e36aa8f63e8"),
            new TesseractLanguage("uig.traineddata.gz", "uig", "Uighur", 0.72, "d20262f24476229539b4b87efa9327428052b241"),
            new TesseractLanguage("ukr.traineddata.gz", "ukr", "Ukrainian", 2.92, "0871744dfacfa446e212e5c7e671c790b5fdd2f0"),
            new TesseractLanguage("urd.traineddata.gz", "urd", "Urdu", 1.83, "be2964ca83114ee04b3a258e71525b8a1a670c97", true),
            new TesseractLanguage("uzb.traineddata.gz", "uzb", "Uzbek", 1.55, "8de3127c90628514d61c0ded9510d4b2728f4b69"),
            new TesseractLanguage("uzb_cyrl.traineddata.gz", "uzb_cyrl", "Uzbek (Cyrillic)", 1.19, "e1190d147d6ce3770d768724c82e103b06c93061"),
            new TesseractLanguage("vie.traineddata.gz", "vie", "Vietnamese", 2.27, "571e132cd3ed26f5c33943efe7aa17835d277a15"),
            new TesseractLanguage("yid.traineddata.gz", "yid", "Yiddish", 1.60, "0dbb6e19b660b57283f954eb5183cc2f3677fdda"),
        });

        #endregion

        #region Tesseract 4.00Beta4 Language Data (auto-generated)

        public static readonly TesseractLanguageData V400B4 = new TesseractLanguageData(new[]
        {
            new TesseractLanguage("afr.traineddata.zip", "afr", "Afrikaans", 5.44, "4278120a18e3464194df302f55417afc35415af7"),
            new TesseractLanguage("amh.traineddata.zip", "amh", "Amharic", 5.55, "166219c79a3c92775ac8cc987fba91899dc63f7d"),
            new TesseractLanguage("ara.traineddata.zip", "ara", "Arabic", 2.29, "6a09f2f96ee04d2bf1c887ea10bcaace429908a6", true),
            new TesseractLanguage("asm.traineddata.zip", "asm", "Assamese", 2.82, "fe4b1d832af281a7947ccf86300c9574827d3b50"),
            new TesseractLanguage("aze.traineddata.zip", "aze", "Azerbaijani", 5.69, "aa1092d2931dc0d500bda58987a49d3a9bb6d98d", true),
            new TesseractLanguage("aze_cyrl.traineddata.zip", "aze_cyrl", "Azerbaijani (Cyrillic)", 2.88, "72fd2b3e1d6f3c88b09f238306e89742b2ae6f0f"),
            new TesseractLanguage("bel.traineddata.zip", "bel", "Belarusian", 5.86, "80da0e84413031213eb30c8b4064fb25b2913cad"),
            new TesseractLanguage("ben.traineddata.zip", "ben", "Bengali", 1.84, "b89191580d742a688bf435fa9ff94f9d468c4858"),
            new TesseractLanguage("bod.traineddata.zip", "bod", "Tibetan", 2.44, "45c144a9d5bf1cdbec50fc24bd3d49bb8f9eba95"),
            new TesseractLanguage("bos.traineddata.zip", "bos", "Bosnian", 4.08, "412d5fff06e9faee873e19cfac0d9e6c72a3c7c8"),
            new TesseractLanguage("bre.traineddata.zip", "bre", "Breton", 6.46, "124f6cdd9b44fb49783fb9908777216d5308102e"),
            new TesseractLanguage("bul.traineddata.zip", "bul", "Bulgarian", 4.32, "05be97ef3169fd953175e37e0b91390a39e5c198"),
            new TesseractLanguage("cat.traineddata.zip", "cat", "Catalan", 3.20, "b8cb54105535c07dd4fd7b9aec441cc27f6692f8"),
            new TesseractLanguage("ceb.traineddata.zip", "ceb", "Cebuano", 1.52, "484d250a6863e8e1fed00368f6a62c049b5b972c"),
            new TesseractLanguage("ces.traineddata.zip", "ces", "Czech", 8.43, "69a7b67e1175ccecd39882e7d521872a93ee85c7"),
            new TesseractLanguage("chi_sim.traineddata.zip", "chi_sim", "Chinese (Simplified)", 20.73, "e26f943534443c274c43a81b24ac10f4c277b9e3"),
            new TesseractLanguage("chi_sim_vert.traineddata.zip", "chi_sim_vert", "Chinese (Simplified, Vertical)", 2.81, "898de9e4322bf818ae7e94cbd89834a9ac0fc7e9"),
            new TesseractLanguage("chi_tra.traineddata.zip", "chi_tra", "Chinese (Traditional)", 27.26, "0eb485e9961bad5f4fe1237b13f61165c356532f"),
            new TesseractLanguage("chi_tra_vert.traineddata.zip", "chi_tra_vert", "Chinese (Traditional, Vertical)", 2.70, "dbefb7180af04cf64b630473dd0ed70569369c1b"),
            new TesseractLanguage("chr.traineddata.zip", "chr", "Cherokee", 0.92, "387d14e948dafe053c644e60a2429f4523e743a3"),
            new TesseractLanguage("cos.traineddata.zip", "cos", "Corsican", 2.64, "892ec8f2156de1d1dd1812b730cbaea59f645097"),
            new TesseractLanguage("cym.traineddata.zip", "cym", "Welsh", 3.97, "42db973712f4949012405295af0cd5eac09b73df"),
            new TesseractLanguage("dan.traineddata.zip", "dan", "Danish", 5.72, "62b39c7b7eb560f2b1910b7e2abbcf62c5b9c882"),
            new TesseractLanguage("dan_frak.traineddata.zip", "dan_frak", "Danish (Fraktur)", 0.65, "becc87d384ddc8f410d5d68ef8c2644bd79fa2ee"),
            new TesseractLanguage("deu.traineddata.zip", "deu", "German", 7.58, "22566b9236a55c3f93324ac78ce26a09c0c4aecc"),
            new TesseractLanguage("deu_frak.traineddata.zip", "deu_frak", "German (Fraktur)", 0.78, "5cd0fbf328e0c6c99f3e7bdd0b8b79ac78166f58"),
            new TesseractLanguage("div.traineddata.zip", "div", "Maldivian", 1.70, "ac91a1e0c11529e958c033394d39ca727fd72091", true),
            new TesseractLanguage("dzo.traineddata.zip", "dzo", "Dzongkha", 0.75, "2d5493a2157d1cf910b4fc8b3e0d861e25dd918a"),
            new TesseractLanguage("ell.traineddata.zip", "ell", "Greek", 3.93, "0e3f029af86d83bbf1291bbc58d574829b4df053"),
            new TesseractLanguage("eng.traineddata.zip", "eng", "English", 12.29, "64d9aa9654d5ee9e82d9b693c3445a91ffbd7b93"),
            new TesseractLanguage("enm.traineddata.zip", "enm", "English (Middle)", 4.58, "06c7e1b3f4135290eae297aece61b11a413de4ba"),
            new TesseractLanguage("epo.traineddata.zip", "epo", "Esperanto", 6.50, "7e4b1cc89c5fcbd9f2b2ae8caad09aa025452f1f"),
            new TesseractLanguage("equ.traineddata.zip", "equ", "Math / equation detection", 0.79, "b15b9a1c006cebac5ffc35569fe01b3e7ee53e72"),
            new TesseractLanguage("est.traineddata.zip", "est", "Estonian", 8.49, "6aefab9f0bdc0c080ee681d3fd359ecef3cd9269"),
            new TesseractLanguage("eus.traineddata.zip", "eus", "Basque", 6.07, "c12b5e15c5bd89e11029c071050d093b2ba188c5"),
            new TesseractLanguage("fao.traineddata.zip", "fao", "Faroese", 3.69, "a7da4c8c4c299557a655e0e5c1045e5beace9c9b"),
            new TesseractLanguage("fas.traineddata.zip", "fas", "Persian", 0.71, "a3aadf776fc9248444d68c971066cf3f86d3c4ce", true),
            new TesseractLanguage("fil.traineddata.zip", "fil", "Filipino", 2.31, "513edccb087eed2771f67c355d4c4938c8811e75"),
            new TesseractLanguage("fin.traineddata.zip", "fin", "Finnish", 12.19, "45db275878b1e73777b5525dc2f01c8f1d3115fa"),
            new TesseractLanguage("fra.traineddata.zip", "fra", "French", 6.55, "0ac9eeb04b334ef29c6a63e86c82aa91d0fe6fce"),
            new TesseractLanguage("frk.traineddata.zip", "frk", "Frankish", 13.08, "2a22d40a403a4e03017de8ae97d4800dbcf21e06"),
            new TesseractLanguage("frm.traineddata.zip", "frm", "French (Middle)", 8.30, "9ad7ab932c2b140e2dc665121ae4ab54e6df0026"),
            new TesseractLanguage("fry.traineddata.zip", "fry", "Frisian (Western)", 2.42, "a455b00abd59502f6f6be2cddd11ddded3c6302e"),
            new TesseractLanguage("gla.traineddata.zip", "gla", "Gaelic", 3.33, "f1f002acf9bb3d17b97e044ddfd654211364129c"),
            new TesseractLanguage("gle.traineddata.zip", "gle", "Irish", 2.55, "fd95035f971a61472be035b977f91865275d2b4f"),
            new TesseractLanguage("glg.traineddata.zip", "glg", "Galician", 5.38, "ab121f2c06eae328335eff6086f3da32549ac9f1"),
            new TesseractLanguage("grc.traineddata.zip", "grc", "Greek (Ancient)", 3.98, "2aec27b8494b63be72e8b493184ee83e75399ffd"),
            new TesseractLanguage("guj.traineddata.zip", "guj", "Gujarati", 1.88, "8365aa9722bb76774bb0648ac23233ed889464a1"),
            new TesseractLanguage("hat.traineddata.zip", "hat", "Haitian", 3.40, "6b2eb7d203d7fd12ee4d283f3a096d2efa9eb1f1"),
            new TesseractLanguage("heb.traineddata.zip", "heb", "Hebrew", 2.53, "8a083a920a85148966472b23f0fef57aa25d49d8", true),
            new TesseractLanguage("hin.traineddata.zip", "hin", "Hindi", 2.22, "469b764d3af97d39fb175ae1ace182033a986706"),
            new TesseractLanguage("hrv.traineddata.zip", "hrv", "Croatian", 7.23, "9d53c8d5c97ff8f40c2bd953a468a170d163ac77"),
            new TesseractLanguage("hun.traineddata.zip", "hun", "Hungarian", 9.58, "adfc68325b8fb215b069e0f093ef96e68d0d068f"),
            new TesseractLanguage("hye.traineddata.zip", "hye", "Armenian", 2.97, "a058b8ec58653ac3cad34823b4f8aec04fb970d9"),
            new TesseractLanguage("iku.traineddata.zip", "iku", "Inuktitut", 3.10, "2a8927e92e3af0d45550ff8a2215310ea9b4bc35"),
            new TesseractLanguage("ind.traineddata.zip", "ind", "Indonesian", 4.24, "d3c3b32e71d2fac63661cbcc842927d9a2825be8"),
            new TesseractLanguage("isl.traineddata.zip", "isl", "Icelandic", 4.96, "9360af20b740d2313863dbe527fd831704bf2121"),
            new TesseractLanguage("ita.traineddata.zip", "ita", "Italian", 7.80, "916998186f658546c3407201127c588539ab447c"),
            new TesseractLanguage("ita_old.traineddata.zip", "ita_old", "Italian (Old)", 8.80, "b6a7efe00f7ce34f75b4a3a91c2c1d3ea83133ca"),
            new TesseractLanguage("jav.traineddata.zip", "jav", "Javanese", 4.74, "5dd426e68a1a2ca4d6a6a771226e54a1a943a61e"),
            new TesseractLanguage("jpn.traineddata.zip", "jpn", "Japanese", 16.77, "73b54f8cd99edffa20627583a82add77462f593a"),
            new TesseractLanguage("jpn_vert.traineddata.zip", "jpn_vert", "Japanese (Vertical)", 3.88, "ff0e822c64c0ba88f9cd4caf6bc90187446da6f2"),
            new TesseractLanguage("kan.traineddata.zip", "kan", "Kannada", 3.66, "7b6e48a0674c2adb39b1b8819751e7fdf1b54722"),
            new TesseractLanguage("kat.traineddata.zip", "kat", "Georgian", 4.29, "c55fb40f2375c91d35409af690d8f139b92a3903"),
            new TesseractLanguage("kat_old.traineddata.zip", "kat_old", "Georgian (Old)", 0.92, "4c94b5f3c90e8034536a7dc1f1e74ec66cfa1bb5"),
            new TesseractLanguage("kaz.traineddata.zip", "kaz", "Kazakh", 5.70, "e07e7ffb3c656c15637b23e56cd95dc1c584a059"),
            new TesseractLanguage("khm.traineddata.zip", "khm", "Khmer (Central)", 2.05, "94b300a9051018506026bfb58ce95da9bc0bd00a"),
            new TesseractLanguage("kir.traineddata.zip", "kir", "Kirghiz", 10.46, "d3d8cc2168427f6dfe584349109bf18be05f6461"),
            new TesseractLanguage("kor.traineddata.zip", "kor", "Korean", 7.66, "8e7dfdf16af0abd98ba87dbb5db59f140fd0429e"),
            new TesseractLanguage("kor_vert.traineddata.zip", "kor_vert", "Korean (Vertical)", 1.18, "76349e042e19e5ed4bffcd4ed6f56159b2620536"),
            new TesseractLanguage("kur.traineddata.zip", "kur", "Kurdish", 0.73, "3dd03488c9e05b6dcca8767c3b3d0d375a214723", true),
            new TesseractLanguage("kur_ara.traineddata.zip", "kur_ara", "Kurdish (Arabic)", 1.83, "a3e0c096cda284b963dce271c358174449fea4dc"),
            new TesseractLanguage("lao.traineddata.zip", "lao", "Lao", 6.52, "0c577c9b9b57a5312dc5cfe1ee3bbf5d728e5b50"),
            new TesseractLanguage("lat.traineddata.zip", "lat", "Latin", 5.38, "5296894c777b799199ecbab99e5880e814fbab5e"),
            new TesseractLanguage("lav.traineddata.zip", "lav", "Latvian", 5.32, "83b04ff7616468868bba6f7c9a7e071d79ddc90f"),
            new TesseractLanguage("lit.traineddata.zip", "lit", "Lithuanian", 6.42, "9d244f95eceee451b54274159fddf84739ec7294"),
            new TesseractLanguage("ltz.traineddata.zip", "ltz", "Luxembourgish", 3.49, "2f1ed3052e57dc7dbf548d1cdd96e14a696f882a"),
            new TesseractLanguage("mal.traineddata.zip", "mal", "Malayalam", 4.73, "11616e5cf327229775b99eb48c71a9733cb18eac"),
            new TesseractLanguage("mar.traineddata.zip", "mar", "Marathi", 2.84, "daa3124cd616bbbab1d1540d65fea8c943f824d5"),
            new TesseractLanguage("mkd.traineddata.zip", "mkd", "Macedonian", 2.83, "acaf7cef9c12557db3a5ece01e2f84c67948bf9b"),
            new TesseractLanguage("mlt.traineddata.zip", "mlt", "Maltese", 4.17, "5629bbe2c8bd96ef0a8fb0cc1fbd969c2bb59496"),
            new TesseractLanguage("mon.traineddata.zip", "mon", "Mongolian", 2.53, "ad42b4564c70088b59802bfc562d04bc6a84ef71"),
            new TesseractLanguage("mri.traineddata.zip", "mri", "Maori", 1.05, "b049cf217b38183855630823e3353259d5d1dd2c"),
            new TesseractLanguage("msa.traineddata.zip", "msa", "Malay", 4.73, "c3f2017c05cc0d6b96f525430625a4b08db63c6e"),
            new TesseractLanguage("mya.traineddata.zip", "mya", "Burmese", 5.15, "16fef298116a5c90e08e8d360322a75d0a394272"),
            new TesseractLanguage("nep.traineddata.zip", "nep", "Nepali", 2.04, "54c5f5db4207ce9254a317cc250bf6eecbd9447d"),
            new TesseractLanguage("nld.traineddata.zip", "nld", "Dutch", 12.59, "56d37209c62e9e6afa51d1e001886564fdd6c45e"),
            new TesseractLanguage("nor.traineddata.zip", "nor", "Norwegian", 7.45, "f0466e0973265352dd37ac7d8a25ad6b76d0a0ee"),
            new TesseractLanguage("oci.traineddata.zip", "oci", "Occitan", 6.09, "915f3df3502995e2867627c5bde58c83644ba796"),
            new TesseractLanguage("ori.traineddata.zip", "ori", "Oriya", 2.04, "2da32dc862e1fc074185fd97f09c0a55edefaf93"),
//            new TesseractLanguage { Filename = "osd.traineddata.zip", Code = "osd", LangName = "", Size = 8.22, Sha1 = "8162903ddc718157e6feeabbfdafe0e375a38001" },
            new TesseractLanguage("pan.traineddata.zip", "pan", "Panjabi", 1.66, "c29528e151531a9891904331f8e320d329a3dd92"),
            new TesseractLanguage("pol.traineddata.zip", "pol", "Polish", 9.89, "8c8e6a3521e17c671defc04607808af97556d07b"),
            new TesseractLanguage("por.traineddata.zip", "por", "Portuguese", 7.38, "58a8b3cddd0c0bf516bf82b7464378662d7e80f5"),
            new TesseractLanguage("pus.traineddata.zip", "pus", "Pushto", 2.73, "83c093d6d2c821d6d9a3f734f753001b2590614a"),
            new TesseractLanguage("que.traineddata.zip", "que", "Quechua", 4.93, "46ab85ef746d6cc0130a9ba5756fb56a250758e4"),
            new TesseractLanguage("ron.traineddata.zip", "ron", "Romanian", 5.65, "d9e931572522802046750d5110ac7aa9d78c816c"),
            new TesseractLanguage("rus.traineddata.zip", "rus", "Russian", 9.74, "949a12e51f29aa02dbcd7e1f41d547780876c335"),
            new TesseractLanguage("san.traineddata.zip", "san", "Sanskrit", 10.86, "13129ccc5fd154f69e1632ed2bdfad3785d0f944"),
            new TesseractLanguage("sin.traineddata.zip", "sin", "Sinhala", 2.19, "7d3a2c6208a4562db3e97a7e0313bd8b9cbf52a2"),
            new TesseractLanguage("slk.traineddata.zip", "slk", "Slovakian", 7.50, "7cdbd545c966a281d0d8954187e1ee612b6f6d65"),
            new TesseractLanguage("slk_frak.traineddata.zip", "slk_frak", "Slovakian (Fraktur)", 0.28, "050b6b8515e7e252b86a121207c205a574e9cd5b"),
            new TesseractLanguage("slv.traineddata.zip", "slv", "Slovenian", 4.93, "0b20f99d0a755db2faffe1508940b166b06835af"),
            new TesseractLanguage("snd.traineddata.zip", "snd", "Sindhi", 2.70, "afc0abcb26a75d833452f0d313f8d428dcfe2613"),
            new TesseractLanguage("spa.traineddata.zip", "spa", "Spanish", 9.03, "21a32e0e3981bb0d62836327567361327173e0cc"),
            new TesseractLanguage("spa_old.traineddata.zip", "spa_old", "Spanish (Old)", 9.79, "5d9e6c07d573f47e90443034e2b2505527315abb"),
            new TesseractLanguage("sqi.traineddata.zip", "sqi", "Albanian", 4.13, "806024671905452d5c4844e15fcccb86863c5563"),
            new TesseractLanguage("srp.traineddata.zip", "srp", "Serbian", 3.92, "30a4f7cc1ddff1154fe49d2a6e7a0edb747a9ec8"),
            new TesseractLanguage("srp_latn.traineddata.zip", "srp_latn", "Serbian (Latin)", 5.65, "8d7f141429265ac927f1f05f4b7ae4b397a8c4ca"),
            new TesseractLanguage("sun.traineddata.zip", "sun", "Sundanese", 1.46, "92643e9e815574d99a125406923bc96c1581bc41"),
            new TesseractLanguage("swa.traineddata.zip", "swa", "Swahili", 3.52, "fc19a0dc5a7047d134e519cf5b0a7a5f2bbcb34e"),
            new TesseractLanguage("swe.traineddata.zip", "swe", "Swedish", 8.42, "d30dbe87e640bd7e95265bd4372ccb0f83722baa"),
            new TesseractLanguage("syr.traineddata.zip", "syr", "Syriac", 3.09, "2047b388123d3511e76a21441458725ec8922658"),
            new TesseractLanguage("tam.traineddata.zip", "tam", "Tamil", 2.65, "8febcf0011ad2642d428cc02915190622e0d9381"),
            new TesseractLanguage("tat.traineddata.zip", "tat", "Tatar", 1.74, "0aa474fdb1dcb8c6b634b366e85dcc4620c4c7fe"),
            new TesseractLanguage("tel.traineddata.zip", "tel", "Telugu", 2.85, "3b0ee160a7af431a3eefabdba48b600db41b8148"),
            new TesseractLanguage("tgk.traineddata.zip", "tgk", "Tajik", 2.62, "83c832eadbb937ef6bd707c07bc43bccc246accd"),
            new TesseractLanguage("tgl.traineddata.zip", "tgl", "Tagalog", 3.13, "a0fdf7c7b935e33260aee265c20b96d0b90d5b08"),
            new TesseractLanguage("tha.traineddata.zip", "tha", "Thai", 1.73, "1289ca3585658dbba7429621d8ab8833c872cafc"),
            new TesseractLanguage("tir.traineddata.zip", "tir", "Tigrinya", 1.18, "5aacd48843a01270729954fac165e215345d1439"),
            new TesseractLanguage("ton.traineddata.zip", "ton", "Tonga (Tonga Islands)", 1.13, "00a679fb18715dc2cb3bda6fa6ce682519f35f9d"),
            new TesseractLanguage("tur.traineddata.zip", "tur", "Turkish", 9.58, "42993630cc2ca6e77743decce6db17c937d4d565"),
            new TesseractLanguage("uig.traineddata.zip", "uig", "Uighur", 3.55, "b4d47e24f7f1f35db23450efa59d5aad20aab8a4"),
            new TesseractLanguage("ukr.traineddata.zip", "ukr", "Ukrainian", 6.48, "fa30fb31bd68d252974fa0902b13a233c3860e49"),
            new TesseractLanguage("urd.traineddata.zip", "urd", "Urdu", 1.97, "e3288ad91bef0987b97c2f465b1d5ad918bd8a01", true),
            new TesseractLanguage("uzb.traineddata.zip", "uzb", "Uzbek", 7.48, "425c50636d22815508ec4d9c78b7218294151bcf"),
            new TesseractLanguage("uzb_cyrl.traineddata.zip", "uzb_cyrl", "Uzbek (Cyrillic)", 2.78, "6a8ac1df9932528848c07b13be15526cea22d458"),
            new TesseractLanguage("vie.traineddata.zip", "vie", "Vietnamese", 4.06, "f3d67cc479ae535393d6a544aae8752a718878c4"),
            new TesseractLanguage("yid.traineddata.zip", "yid", "Yiddish", 2.38, "fbaf27e063c45fb366dc5cf38a472b616fc2553a"),
            new TesseractLanguage("yor.traineddata.zip", "yor", "Yoruba", 1.14, "b7bcc0416531f0432af9ed523887d0aa0dfb272b"),
        });

        #endregion
    }
}
