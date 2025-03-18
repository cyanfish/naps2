namespace NAPS2.Ocr;

public class TesseractLanguageData
{
    public class TesseractLanguage
    {
        public TesseractLanguage(string filename, string code, string langName, double size, string sha256, bool rtl = false)
        {
            Filename = filename;
            Code = code;
            LangName = langName;
            Size = size;
            Sha256 = sha256;
            RTL = rtl;
        }
            
        public string Filename { get; }

        public string Code { get; }

        public string LangName { get; }

        public double Size { get; }

        public string Sha256 { get; }

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
        new TesseractLanguage("afr.traineddata.zip", "afr", "Afrikaans", 5.44, "bb6a056b43944df862815fb173c9c582ec4410c2794cfd6b632cb31611d0476a"),
        new TesseractLanguage("amh.traineddata.zip", "amh", "Amharic", 5.55, "4d79215805af87c39720d036e3d005e781d49ef7bc309d40824ab689242cc168"),
        new TesseractLanguage("ara.traineddata.zip", "ara", "Arabic", 2.29, "24d2a74937150e6d41a69f2758aea3817d7f80ed53f780adaa492a83e249b62f", true),
        new TesseractLanguage("asm.traineddata.zip", "asm", "Assamese", 2.82, "ae1f04d3a4f5324c223a9330f5dd0836bd926fde70cae5d2362e10c77af99750"),
        new TesseractLanguage("aze.traineddata.zip", "aze", "Azerbaijani", 5.69, "c9b7e5d86c7aa95cdfc5ba0f475c21a43793811761b890c54610c76a9d8c6027", true),
        new TesseractLanguage("aze_cyrl.traineddata.zip", "aze_cyrl", "Azerbaijani (Cyrillic)", 2.88, "b4fc131f77af3b04729e614021b02b993d88b6e8dc6b856c2e890e8da5de44b2"),
        new TesseractLanguage("bel.traineddata.zip", "bel", "Belarusian", 5.86, "67c36d8b9886a2e6412f31950b2a5daeae8196f5bb73a48782eb273b354dc1fc"),
        new TesseractLanguage("ben.traineddata.zip", "ben", "Bengali", 1.84, "814d18592e4bae3ce62ffb680cb242f5ddf94b57f825ce2e60acad20d4eb0d39"),
        new TesseractLanguage("bod.traineddata.zip", "bod", "Tibetan", 2.44, "4b0d1db9cfc932d4cf9d8fb0d140362c3f70b21e4c12cca595c9674e90ce9495"),
        new TesseractLanguage("bos.traineddata.zip", "bos", "Bosnian", 4.08, "b996240751fcc05415b56db5809c05383cdaf47d2eddd51e2a5e20445f8111d3"),
        new TesseractLanguage("bre.traineddata.zip", "bre", "Breton", 6.46, "c9485e27c69a834f3e61f502f92453387e8975aceaa601538c7d94c7ccb02d2b"),
        new TesseractLanguage("bul.traineddata.zip", "bul", "Bulgarian", 4.32, "75efd60898e3f6e254f3a9059d53548b92140ff14c603cbc3d46061d7a2d257d"),
        new TesseractLanguage("cat.traineddata.zip", "cat", "Catalan", 3.20, "e8897072d0b5f0b37834083bddecb08a71374c39c19daba14ae5e905dcdad3b0"),
        new TesseractLanguage("ceb.traineddata.zip", "ceb", "Cebuano", 1.52, "cb0f10ae09ea0b4df4c0d5ddc8fe7ce370a7fb4d6ff3e23a8d96a70b1a36ccba"),
        new TesseractLanguage("ces.traineddata.zip", "ces", "Czech", 8.43, "fd9eb6ec0e646f48c2786bdc508b8a9e59d3d1ca034eff1dfbc930e1864fe9ba"),
        new TesseractLanguage("chi_sim.traineddata.zip", "chi_sim", "Chinese (Simplified)", 20.73, "21ebe92da47078c237b776d5ef79b3668b6a5b76f1ec527588e60d57230f097d"),
        new TesseractLanguage("chi_sim_vert.traineddata.zip", "chi_sim_vert", "Chinese (Simplified, Vertical)", 2.81, "6d1a3352fca9b0d5e54a40e29e97d7baaa1e459146954dcfa54a03cf7c3d4893"),
        new TesseractLanguage("chi_tra.traineddata.zip", "chi_tra", "Chinese (Traditional)", 27.26, "a2127d36753638ab6a018d544b4f518b9b1cf68738f8da586e4004a22eb16500"),
        new TesseractLanguage("chi_tra_vert.traineddata.zip", "chi_tra_vert", "Chinese (Traditional, Vertical)", 2.70, "83d0c40f93bc801e48b3a3ca7490f7477742dae17b96496d1256b2442c5c50b8"),
        new TesseractLanguage("chr.traineddata.zip", "chr", "Cherokee", 0.92, "116a17bbb257975708595a6f5b6f59e3e593a58397cca603fd141cda51285758"),
        new TesseractLanguage("cos.traineddata.zip", "cos", "Corsican", 2.64, "28abc11555b277c4d5f7ed928de06867ebd8b4a1f6e61ec85cc16fc783cbca4e"),
        new TesseractLanguage("cym.traineddata.zip", "cym", "Welsh", 3.97, "c3d12e77a487a4e2b3200f890236e2b3842a3102856f5dd57a6cd4a325b26603"),
        new TesseractLanguage("dan.traineddata.zip", "dan", "Danish", 5.72, "e778c64ef4c3d39d3dd4d38cf5a3e7adabd19adaa291248893e3e3ee7fcf75d7"),
        new TesseractLanguage("deu.traineddata.zip", "deu", "German", 7.58, "134f41cb8a47e139cf988ea255f03a5b278d50eabad438204f708abf5a6442ce"),
        new TesseractLanguage("deu_latf.traineddata.zip", "deu_latf", "German (Fraktur)", 12.45, "285f53f9cbe1ea79da37d087feabb4caac94e551e8d3e4f476b392c81c43fcf6"),
        new TesseractLanguage("div.traineddata.zip", "div", "Maldivian", 1.70, "e5d67c91ef76570aa05e068a292d6f658c0a84ccf73d97ba2f94edd7cdbea0af", true),
        new TesseractLanguage("dzo.traineddata.zip", "dzo", "Dzongkha", 0.75, "f890900540ddf88909954639f35689ffc9d40c78af27d70600fc510302162857"),
        new TesseractLanguage("ell.traineddata.zip", "ell", "Greek", 3.93, "9dede46367708f74917386b88a08e7ca41829aa4278a0b187af594fc6a584ae8"),
        new TesseractLanguage("eng.traineddata.zip", "eng", "English", 12.29, "9d167994617c5fd827bf6b31f298e69cd05e6d30b9965715e78191df5606389e"),
        new TesseractLanguage("enm.traineddata.zip", "enm", "English (Middle)", 4.58, "248b730c6b77634f1679a6411a02e069f6c7b7da1374f77a173eb2e00b34081f"),
        new TesseractLanguage("epo.traineddata.zip", "epo", "Esperanto", 6.50, "fc6630ede1d9cbfe84ddb2889fd2fe7470313cfb3087598d71cc7c4e18d0c837"),
        new TesseractLanguage("equ.traineddata.zip", "equ", "Math / equation detection", 0.79, "b18f660b80fa9d352fe8d2261e14bd3463b4037e72c49b9fc88c01297ac1a95e"),
        new TesseractLanguage("est.traineddata.zip", "est", "Estonian", 8.49, "e9f38932eb9497a686f70ed8b586d0fef920cb140ff7b9978f32349a0db46d19"),
        new TesseractLanguage("eus.traineddata.zip", "eus", "Basque", 6.07, "0e3bce4aa08bcc4c53fb6d5e8884d1e973406c086af1bec947c96b6348742d16"),
        new TesseractLanguage("fao.traineddata.zip", "fao", "Faroese", 3.69, "4999b8f539cc1a432cdc52ac69cbb21654da994f292f42df4d4a025ae27623b3"),
        new TesseractLanguage("fas.traineddata.zip", "fas", "Persian", 0.71, "3f3d75e2f645a262241c80b79e799f0a9523fadcf4958d1170a583972c124ce0", true),
        new TesseractLanguage("fil.traineddata.zip", "fil", "Filipino", 2.31, "acb7b499d00e65087275d53e997444e5a32d49ffe543534e254cfc45d38274b3"),
        new TesseractLanguage("fin.traineddata.zip", "fin", "Finnish", 12.19, "e16d491a9ee90e66786624ee1e8e3db0c57ad334162b888dc06e33340f697d47"),
        new TesseractLanguage("fra.traineddata.zip", "fra", "French", 6.55, "00b779d1373d837f927d34f1e66d9b99d0339ce5dba82d80b928c5eba431ad8a"),
        new TesseractLanguage("frm.traineddata.zip", "frm", "French (Middle)", 8.30, "2339bf1c78224ff827eca19e2d77d01b70599733fe4fc972b75127a442870ef6"),
        new TesseractLanguage("fry.traineddata.zip", "fry", "Frisian (Western)", 2.42, "0106765e7634dd3e2b2c0587eb66635cf647cb90ed21cfc1434e5ab191a4f702"),
        new TesseractLanguage("gla.traineddata.zip", "gla", "Gaelic", 3.33, "4543b56903859324ac465e8b238346c2aca653479fba93e1fdb0bbe22583de66"),
        new TesseractLanguage("gle.traineddata.zip", "gle", "Irish", 2.55, "e9981b837e37c3e9f25cebc1904eab35a2c1cb2dcb882fefe3f14b3e224b2538"),
        new TesseractLanguage("glg.traineddata.zip", "glg", "Galician", 5.38, "cecfdfc25bd2f31a303ddeaab4158eee8b93d355205ade8a749d6698f8cbcf06"),
        new TesseractLanguage("grc.traineddata.zip", "grc", "Greek (Ancient)", 3.98, "c66c23954dd2d950b27d70966ed56373b6ee75a41bcfddf6a9e032c951ea0312"),
        new TesseractLanguage("guj.traineddata.zip", "guj", "Gujarati", 1.88, "9584daea9406462710e22d034b45367afec73ecbea966b42fd9fbcb79b9c3116"),
        new TesseractLanguage("hat.traineddata.zip", "hat", "Haitian", 3.40, "3f985b227a7fd53a57be8e0cd8de0e09e8b9f995cf88671338dfca52119ce7c7"),
        new TesseractLanguage("heb.traineddata.zip", "heb", "Hebrew", 2.53, "66af43320edf32521cb3b86d5362f1f98ccbcb843f98d575267c83a4a94de4f9", true),
        new TesseractLanguage("hin.traineddata.zip", "hin", "Hindi", 2.22, "4d5d7d7b5851815ce27471e153633c343f227f2ce2cb08e8ac6ff7955c8847e1"),
        new TesseractLanguage("hrv.traineddata.zip", "hrv", "Croatian", 7.23, "4f263d15d567dfea924338e02ebfaf37ab500f8ee1f121f7c0e5f73ee577fb89"),
        new TesseractLanguage("hun.traineddata.zip", "hun", "Hungarian", 9.58, "63ad9219c6ceb4aab27fb937fc4bc7c40f803bd8f3a58e94a796b695be89c2b3"),
        new TesseractLanguage("hye.traineddata.zip", "hye", "Armenian", 2.97, "96fc9d9883b9743095ebdc11aa3c3843909f0b0d77a91ce69a4adc82609b6c60"),
        new TesseractLanguage("iku.traineddata.zip", "iku", "Inuktitut", 3.10, "72a7fa284e0b8412b9e7c5ad11f474d26b67c8c9e1cb576d2bc334273e1503bb"),
        new TesseractLanguage("ind.traineddata.zip", "ind", "Indonesian", 4.24, "945916910f6c3a2a2a0bae1323e0509ca04a3e01f0daf2c43d90180cca642edd"),
        new TesseractLanguage("isl.traineddata.zip", "isl", "Icelandic", 4.96, "1a984bfc814cb1cbc83d97a3e887ccee20d1e1c5eac0b624bb1f8121d781cd97"),
        new TesseractLanguage("ita.traineddata.zip", "ita", "Italian", 7.80, "53136c0def889f2acad686e96e6f40b7a7d53e5537dfbbf956ba351892bbf8e6"),
        new TesseractLanguage("ita_old.traineddata.zip", "ita_old", "Italian (Old)", 8.80, "d4629a523f749e1c79ba391fbb45809b886b0357c2af78634603c1126a1a6621"),
        new TesseractLanguage("jav.traineddata.zip", "jav", "Javanese", 4.74, "a51fa159417e0d502d0f0e9ca845560ec58223d0c5507f2482558f22e04cfd39"),
        new TesseractLanguage("jpn.traineddata.zip", "jpn", "Japanese", 16.77, "7ce9a993f7fb67099483d126820352768001ca356975ba4a691c23fe7c3e2245"),
        new TesseractLanguage("jpn_vert.traineddata.zip", "jpn_vert", "Japanese (Vertical)", 3.88, "da584c3e0368ddcad57fa82a7a4063af778bf7fd941675ca7c747c3492bc64e5"),
        new TesseractLanguage("kan.traineddata.zip", "kan", "Kannada", 3.66, "884f6ee097c45ebb2b28e52c8763c9220cdbfe4899b464b8f3eb8fd32471ffac"),
        new TesseractLanguage("kat.traineddata.zip", "kat", "Georgian", 4.29, "45c0978cef610a6348f67e2fc89f0d6021a2ef611f6c45a515e413b5dc72c9e1"),
        new TesseractLanguage("kat_old.traineddata.zip", "kat_old", "Georgian (Old)", 0.92, "ff05139f6eabfba3e99ea392036da63a3e432fca2ab3e815d952edf72c232353"),
        new TesseractLanguage("kaz.traineddata.zip", "kaz", "Kazakh", 5.70, "39e0e34b750982b1e0bde53b88130dae8d6a51c12a95ae7ac8688b0d9ddc0398"),
        new TesseractLanguage("khm.traineddata.zip", "khm", "Khmer (Central)", 2.05, "334e251c0674e7105eda8ab910c064735f53ba91ed549f14b89652ce2befed7b"),
        new TesseractLanguage("kir.traineddata.zip", "kir", "Kirghiz", 10.46, "c7e26b19ee7e49c1912c4abafa8aac42ebea7516b26a779ed4fe7982c6885eb9"),
        new TesseractLanguage("kor.traineddata.zip", "kor", "Korean", 7.66, "637fb7623cb4f9d255c9a7366070b2efccc20aa1895d76cc6abda63cc76ce0f9"),
        new TesseractLanguage("kor_vert.traineddata.zip", "kor_vert", "Korean (Vertical)", 1.18, "19be9d55142b0fec23c065ddceae763f42b4b8e5802a9e7433a04b83cb3c66eb"),
        new TesseractLanguage("kmr.traineddata.zip", "kmr", "Kurmanji", 9.69, "40385118857473589a3ddfb280114f89309dc45205657132d16805d678e07129"),
        new TesseractLanguage("lao.traineddata.zip", "lao", "Lao", 6.52, "338e3cfa48970a97cd2be6f64dd160b3b1865ea47dc3add0b88abe0ccbc81e1b"),
        new TesseractLanguage("lat.traineddata.zip", "lat", "Latin", 5.38, "733ae53125ec83615cb636de9768b48998c830c5d9826f3cad3f0dd3b4be3ec7"),
        new TesseractLanguage("lav.traineddata.zip", "lav", "Latvian", 5.32, "c92311b0f4d04919c95203abb9f457c050dadea100ef769afc831aa5ead5c97f"),
        new TesseractLanguage("lit.traineddata.zip", "lit", "Lithuanian", 6.42, "5b31dc6b4ace6d0a89aff824449057c38261ca0a46187fec658cd88c0e528f7e"),
        new TesseractLanguage("ltz.traineddata.zip", "ltz", "Luxembourgish", 3.49, "8a71a214502fb9ee25bf3112182473aa512d9dc9bb2cddb2d5864655e038db62"),
        new TesseractLanguage("mal.traineddata.zip", "mal", "Malayalam", 4.73, "7ab1f2013f48af8c2a3852394735d1726d3b9f0c643179f26ac424655633bc9f"),
        new TesseractLanguage("mar.traineddata.zip", "mar", "Marathi", 2.84, "60a366dc46f88dab81abaef7409e39f7ec90aa61f594914df4d76fbfb0f48d57"),
        new TesseractLanguage("mkd.traineddata.zip", "mkd", "Macedonian", 2.83, "a4898b829bc6c5a9b70f39b56b8c4daa17c3984011ed50286bd7a62d520704d8"),
        new TesseractLanguage("mlt.traineddata.zip", "mlt", "Maltese", 4.17, "56c59a7a78e98d4a228f3d15f29adaef92433e577fb0cdb468f685243c315d3f"),
        new TesseractLanguage("mon.traineddata.zip", "mon", "Mongolian", 2.53, "dac6ff62379b211f97b8e4f8bb507ccafc0ff1c1ee7e0d8bf783fac6d0de4f0d"),
        new TesseractLanguage("mri.traineddata.zip", "mri", "Maori", 1.05, "adffa1d84438509ca06ef0fd6dbe9850e40017e97a119247e535c1b4d7bed6cd"),
        new TesseractLanguage("msa.traineddata.zip", "msa", "Malay", 4.73, "d2e2debc66462e90be6b61554c4d2b0b4f94e0b491e65e520b7dac03078f1e9c"),
        new TesseractLanguage("mya.traineddata.zip", "mya", "Burmese", 5.15, "6b829f14edafdd00f673b5aa9945b430adf8f42a1f15e97940097232b592c81a"),
        new TesseractLanguage("nep.traineddata.zip", "nep", "Nepali", 2.04, "727823700b6f52abeaab649f82a43f0d270ce26630101a77587b0e238a5f476f"),
        new TesseractLanguage("nld.traineddata.zip", "nld", "Dutch", 12.59, "572f6fc496f7998f10e1a229ca103c0f6bb685d837648c04315efc98b98524c3"),
        new TesseractLanguage("nor.traineddata.zip", "nor", "Norwegian", 7.45, "78e3e8884930f529950d4e0ecc2bc52aea0e21092d7d6fc2aa7fedb58ce5530f"),
        new TesseractLanguage("oci.traineddata.zip", "oci", "Occitan", 6.09, "cc643881830d43003fcf507d00e5a08f627e14eb0bbd32ea73f2809f2af2d244"),
        new TesseractLanguage("ori.traineddata.zip", "ori", "Oriya", 2.04, "35cfe0a66de80cdf443b1e997a60a1804a973df94d978e6c03d2a1a84e9e6927"),
//            new TesseractLanguage { Filename = "osd.traineddata.zip", Code = "osd", LangName = "", Size = 8.22, Sha1 = "8162903ddc718157e6feeabbfdafe0e375a38001" },
        new TesseractLanguage("pan.traineddata.zip", "pan", "Panjabi", 1.66, "42bd9b864b13a56ab57bb75cc0998f3aa6e9219270324682667351560695ce91"),
        new TesseractLanguage("pol.traineddata.zip", "pol", "Polish", 9.89, "c7a892bcb49f237159662865fa18703ccc03f0dd8d1573fd8ffc9057d664e962"),
        new TesseractLanguage("por.traineddata.zip", "por", "Portuguese", 7.38, "0f8913e7e691237e0ff946ba993ed32a1c1eafcf7957263dc4a54f1b2a99411d"),
        new TesseractLanguage("pus.traineddata.zip", "pus", "Pushto", 2.73, "2e2eeca1e2791e415e141ba770766145299ba84f978587368708e7624b294a22"),
        new TesseractLanguage("que.traineddata.zip", "que", "Quechua", 4.93, "c45b94df8262c791509a365c49fcd0a451f4b6329790355563e3df2e7ceef772"),
        new TesseractLanguage("ron.traineddata.zip", "ron", "Romanian", 5.65, "79d51616c67ad813608b3a82dd10cfc1ee874463d5dce11cc61c12de3f48a58c"),
        new TesseractLanguage("rus.traineddata.zip", "rus", "Russian", 9.74, "0dd9b6ab808045a706ad312283b1841b1ba0c5e196287e7a773f66e1e629b5e6"),
        new TesseractLanguage("san.traineddata.zip", "san", "Sanskrit", 10.86, "eae9a65200412eae292878956c4e4ef4f9d0e738f7b54a651e5e808f49e5b523"),
        new TesseractLanguage("sin.traineddata.zip", "sin", "Sinhala", 2.19, "0ebf36eda329d05eb4b9f7e829250a51ef6201fa005bce61a2822a026f88f4c0"),
        new TesseractLanguage("slk.traineddata.zip", "slk", "Slovakian", 7.50, "ed7e2cc03e9a293536d356f8e62707448c2376f147b6b8cff4e9b53c9f545659"),
        new TesseractLanguage("slv.traineddata.zip", "slv", "Slovenian", 4.93, "5dc96ba037c189462dd9baa804a6ee1ce00006484f4548fe922045ecb38fb4f0"),
        new TesseractLanguage("snd.traineddata.zip", "snd", "Sindhi", 2.70, "08157a44f943463cbc50e365150881ce4aea88782ad5194e1137e2b7609bd1f1"),
        new TesseractLanguage("spa.traineddata.zip", "spa", "Spanish", 9.03, "820a0ec5e083188b366ac3e5d79f02fa1bb51c274c3e01ce6943826bacc38981"),
        new TesseractLanguage("spa_old.traineddata.zip", "spa_old", "Spanish (Old)", 9.79, "0ec4bed63b5950fb62281cc32b4b307327f8883b5e61ee4d7579d00b18053533"),
        new TesseractLanguage("sqi.traineddata.zip", "sqi", "Albanian", 4.13, "063dc592eed9c5adc41b006b2b52fe84f0103f144499b1a6d4423bd301c375df"),
        new TesseractLanguage("srp.traineddata.zip", "srp", "Serbian", 3.92, "13c63b4992906d5c016924ece6ab29d8b27cc1bf7111dd7243ccb680e799fac2"),
        new TesseractLanguage("srp_latn.traineddata.zip", "srp_latn", "Serbian (Latin)", 5.65, "eb0106cc402d15f1191d3a20be0a8c4fbf18f6e0318be253721860cf87d2b3f6"),
        new TesseractLanguage("sun.traineddata.zip", "sun", "Sundanese", 1.46, "59e4f195d8f861bebd6ced50617e107f6526ae99707f090dab60be5300ec1635"),
        new TesseractLanguage("swa.traineddata.zip", "swa", "Swahili", 3.52, "b1efe3243d6e10b0af8bcdb2f7dce37c31d80c86eef811745a89fa75274c9900"),
        new TesseractLanguage("swe.traineddata.zip", "swe", "Swedish", 8.42, "409610afe61aeb74b91918e1c5a1e0eaa484d942cab231513bcd2e3339e0944f"),
        new TesseractLanguage("syr.traineddata.zip", "syr", "Syriac", 3.09, "401fa37551c581fb3a75681f7dc7dd124f72efbff785293b75b61c34a821525e"),
        new TesseractLanguage("tam.traineddata.zip", "tam", "Tamil", 2.65, "6781e9f4f0a96813a6435429e5f975f8dd06f068daec6226fb5fff8042508ddc"),
        new TesseractLanguage("tat.traineddata.zip", "tat", "Tatar", 1.74, "93e218b0e3362dde43b16cdda4c8617e190c5c008337b0232d6110e1e51bb57c"),
        new TesseractLanguage("tel.traineddata.zip", "tel", "Telugu", 2.85, "19ad3382a1399afa50c7e9a39fb0f0c852fd21f2c4da747042462bd8c8a51d8b"),
        new TesseractLanguage("tgk.traineddata.zip", "tgk", "Tajik", 2.62, "ef35f09f7b1c2dc4883548bd1a0f1d5e3bfe5d0938b7c4ef03e70ea37f3053c2"),
        new TesseractLanguage("tgl.traineddata.zip", "tgl", "Tagalog", 3.13, "3600aeb0eb65c96b34a0625f94009417ff584947fd370628029c1526e5f0d96f"),
        new TesseractLanguage("tha.traineddata.zip", "tha", "Thai", 1.73, "e590d1b4daa75e8d99704174c0a574d5fdf2750952d3d9bae50fa1adb1c5b9be"),
        new TesseractLanguage("tir.traineddata.zip", "tir", "Tigrinya", 1.18, "be42a4f3647745b0af276f6d61ab5348846589cd3efd88c9908efd3d27201aff"),
        new TesseractLanguage("ton.traineddata.zip", "ton", "Tonga (Tonga Islands)", 1.13, "67f5dba9dccbe6b79f9b036afc714dfe065d077237636fcf45e6851b85c549b5"),
        new TesseractLanguage("tur.traineddata.zip", "tur", "Turkish", 9.58, "7141de10ca977abdab21080afe5dc2025d6cec1661fba5803099bacc25033091"),
        new TesseractLanguage("uig.traineddata.zip", "uig", "Uighur", 3.55, "18f9899c48078c59b9acc5b64f198bfd6fca09a24ee9aca4e97efcbd24a8dc27"),
        new TesseractLanguage("ukr.traineddata.zip", "ukr", "Ukrainian", 6.48, "c14a794e98c24976c84f41f05217fba1155953f3a6062b9f63d6122e0936f6f7"),
        new TesseractLanguage("urd.traineddata.zip", "urd", "Urdu", 1.97, "7a83e5e191c40387422c6b6903b7d4e865f35782269b66760876b4f76d1f3cb3", true),
        new TesseractLanguage("uzb.traineddata.zip", "uzb", "Uzbek", 7.48, "9e0502ef7fbd0e45cbd843ad5921784416d149ec47f9c43b01a1236da1e5e064"),
        new TesseractLanguage("uzb_cyrl.traineddata.zip", "uzb_cyrl", "Uzbek (Cyrillic)", 2.78, "b719f58617c56517018cd2c35618622aa8c10c7de2466f64fb0aa452ccb6ef28"),
        new TesseractLanguage("vie.traineddata.zip", "vie", "Vietnamese", 4.06, "3fd45915b39bc97cb23091ae5b87bd0f8ef0400237c31689f3587dafcc69b730"),
        new TesseractLanguage("yid.traineddata.zip", "yid", "Yiddish", 2.38, "798b6bc403f98ba573be631663794ac314ec7083c1871e2a9b0baf214b1c73ff"),
        new TesseractLanguage("yor.traineddata.zip", "yor", "Yoruba", 1.14, "e41bc668af832e13758bc94f8aa9fa9358f34a7e1b16d7193e5f5f1de9e83467"),
    });

    #endregion
}