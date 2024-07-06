namespace NAPS2.Ocr;

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

    #region Tesseract Language Data (auto-generated)

    public static readonly TesseractLanguageData Latest = new(new[]
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
        new TesseractLanguage("deu.traineddata.zip", "deu", "German", 7.58, "22566b9236a55c3f93324ac78ce26a09c0c4aecc"),
        new TesseractLanguage("deu_latf.traineddata.zip", "deu_latf", "German (Fraktur)", 12.45, "8039a9d676f86f545babd046576dee4aa8415e0d"),
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
        new TesseractLanguage("kmr.traineddata.zip", "kmr", "Kurmanji", 9.69, "7be84a7d6dba248272147b97c03a1224f65462cf"),
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