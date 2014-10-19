using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FOcrLanguages : FormBase
    {
        public FOcrLanguages()
        {
            InitializeComponent();

            // English first
            var englishItem = GetItem(Languages[0]);
            englishItem.Checked = true;
            lvLanguages.Items.Add(englishItem);
            // Then everything else in alphabetical order
            lvLanguages.Items.AddRange(Languages.Skip(1).OrderBy(x => x.LangName).Select(GetItem).ToArray());
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(lvLanguages)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(labelSizeEstimate, btnCancel, btnDownload)
                    .BottomToForm()
                .Bind(btnCancel, btnDownload)
                    .RightToForm()
                .Activate();
        }

        private void UpdateView()
        {
            double langDownloadSize =
                lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => ((Language)x.Tag).Size).Sum();
            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, langDownloadSize);

            btnDownload.Enabled = lvLanguages.Items.Cast<ListViewItem>().Any(x => x.Checked);
        }

        private ListViewItem GetItem(Language lang)
        {
            return new ListViewItem { Text = lang.LangName, Tag = lang };
        }

        private void lvLanguages_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateView();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static readonly Language[] Languages =
        {
            new Language { Filename = "tesseract-ocr-3.02.eng.tar.gz", Code = "eng", LangName = "English", Size = 12.1 },
            new Language { Filename = "tesseract-ocr-3.02.epo_alt.tar.gz", Code = "epo_alt", LangName = "Esperanto alternative", Size = 1.4 },
            new Language { Filename = "tesseract-ocr-3.02.ukr.tar.gz", Code = "ukr", LangName = "Ukrainian", Size = 0.9 },
            new Language { Filename = "tesseract-ocr-3.02.tur.tar.gz", Code = "tur", LangName = "Turkish", Size = 3.4 },
            new Language { Filename = "tesseract-ocr-3.02.tha.tar.gz", Code = "tha", LangName = "Thai", Size = 3.7 },
            new Language { Filename = "tesseract-ocr-3.02.tgl.tar.gz", Code = "tgl", LangName = "Tagalog", Size = 1.4 },
            new Language { Filename = "tesseract-ocr-3.02.tel.tar.gz", Code = "tel", LangName = "Telugu", Size = 5.6 },
            new Language { Filename = "tesseract-ocr-3.02.tam.tar.gz", Code = "tam", LangName = "Tamil", Size = 3.4 },
            new Language { Filename = "tesseract-ocr-3.02.swe.tar.gz", Code = "swe", LangName = "Swedish", Size = 2.3 },
            new Language { Filename = "tesseract-ocr-3.02.swa.tar.gz", Code = "swa", LangName = "Swahili", Size = 0.7 },
            new Language { Filename = "tesseract-ocr-3.02.srp.tar.gz", Code = "srp", LangName = "Serbian (Latin)", Size = 1.7 },
            new Language { Filename = "tesseract-ocr-3.02.sqi.tar.gz", Code = "sqi", LangName = "Albanian", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.spa_old.tar.gz", Code = "spa_old", LangName = "Spanish (Old)", Size = 5.4 },
            new Language { Filename = "tesseract-ocr-3.02.spa.tar.gz", Code = "spa", LangName = "Spanish", Size = 9.4 },
            new Language { Filename = "tesseract-ocr-3.02.slv.tar.gz", Code = "slv", LangName = "Slovenian", Size = 1.5 },
            new Language { Filename = "tesseract-ocr-3.02.slk.tar.gz", Code = "slk", LangName = "Slovakian", Size = 2.4 },
            new Language { Filename = "tesseract-ocr-3.02.ron.tar.gz", Code = "ron", LangName = "Romanian", Size = 0.9 },
            new Language { Filename = "tesseract-ocr-3.02.por.tar.gz", Code = "por", LangName = "Portuguese", Size = 0.9 },
            new Language { Filename = "tesseract-ocr-3.02.pol.tar.gz", Code = "pol", LangName = "Polish", Size = 6.7 },
            new Language { Filename = "tesseract-ocr-3.02.nor.tar.gz", Code = "nor", LangName = "Norwegian", Size = 2.1 },
            new Language { Filename = "tesseract-ocr-3.02.nld.tar.gz", Code = "nld", LangName = "Dutch", Size = 1.1 },
            new Language { Filename = "tesseract-ocr-3.02.msa.tar.gz", Code = "msa", LangName = "Malay", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.mlt.tar.gz", Code = "mlt", LangName = "Maltese", Size = 1.4 },
            new Language { Filename = "tesseract-ocr-3.02.mkd.tar.gz", Code = "mkd", LangName = "Macedonian", Size = 1.1 },
            new Language { Filename = "tesseract-ocr-3.02.mal.tar.gz", Code = "mal", LangName = "Malayalam", Size = 5.8 },
            new Language { Filename = "tesseract-ocr-3.02.lit.tar.gz", Code = "lit", LangName = "Lithuanian", Size = 1.7 },
            new Language { Filename = "tesseract-ocr-3.02.lav.tar.gz", Code = "lav", LangName = "Latvian", Size = 1.7 },
            new Language { Filename = "tesseract-ocr-3.02.kor.tar.gz", Code = "kor", LangName = "Korean", Size = 5.2 },
            new Language { Filename = "tesseract-ocr-3.02.kan.tar.gz", Code = "kan", LangName = "Kannada", Size = 4.2 },
            new Language { Filename = "tesseract-ocr-3.02.ita_old.tar.gz", Code = "ita_old", LangName = "Italian (Old)", Size = 3.3 },
            new Language { Filename = "tesseract-ocr-3.02.ita.tar.gz", Code = "ita", LangName = "Italian", Size = 6.8 },
            new Language { Filename = "tesseract-ocr-3.02.isl.tar.gz", Code = "isl", LangName = "Icelandic", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.ind.tar.gz", Code = "ind", LangName = "Indonesian", Size = 1.8 },
            new Language { Filename = "tesseract-ocr-3.02.chr.tar.gz", Code = "chr", LangName = "Cherokee", Size = 0.3 },
            new Language { Filename = "tesseract-ocr-3.02.hun.tar.gz", Code = "hun", LangName = "Hungarian", Size = 3.0 },
            new Language { Filename = "tesseract-ocr-3.02.hrv.tar.gz", Code = "hrv", LangName = "Croatian", Size = 1.8 },
            new Language { Filename = "tesseract-ocr-3.02.hin.tar.gz", Code = "hin", LangName = "Hindi", Size = 9.6 },
            new Language { Filename = "tesseract-ocr-3.02.heb.tar.gz", Code = "heb", LangName = "Hebrew", Size = 1.0 },
            new Language { Filename = "tesseract-ocr-3.02.glg.tar.gz", Code = "glg", LangName = "Galician", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.frm.tar.gz", Code = "frm", LangName = "Middle French (ca. 1400-1600)", Size = 4.9 },
            new Language { Filename = "tesseract-ocr-3.02.frk.tar.gz", Code = "frk", LangName = "Frankish", Size = 5.6 },
            new Language { Filename = "tesseract-ocr-3.02.fra.tar.gz", Code = "fra", LangName = "French", Size = 6.2 },
            new Language { Filename = "tesseract-ocr-3.02.fin.tar.gz", Code = "fin", LangName = "Finnish", Size = 1.0 },
            new Language { Filename = "tesseract-ocr-3.02.eus.tar.gz", Code = "eus", LangName = "Basque", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.est.tar.gz", Code = "est", LangName = "Estonian", Size = 1.8 },
            new Language { Filename = "tesseract-ocr-3.02.equ.tar.gz", Code = "equ", LangName = "Math / equation detection", Size = 0.8 },
            new Language { Filename = "tesseract-ocr-3.02.epo.tar.gz", Code = "epo", LangName = "Esperanto", Size = 1.2 },
            new Language { Filename = "tesseract-ocr-3.02.enm.tar.gz", Code = "enm", LangName = "Middle English (1100-1500)", Size = 0.6 },
            new Language { Filename = "tesseract-ocr-3.02.ell.tar.gz", Code = "ell", LangName = "Greek", Size = 0.8 },
            new Language { Filename = "tesseract-ocr-3.02.deu.tar.gz", Code = "deu", LangName = "German", Size = 1.7 },
            new Language { Filename = "tesseract-ocr-3.02.dan.tar.gz", Code = "dan", LangName = "Danish", Size = 2.5 },
            new Language { Filename = "tesseract-ocr-3.02.ces.tar.gz", Code = "ces", LangName = "Czech", Size = 1.0 },
            new Language { Filename = "tesseract-ocr-3.02.cat.tar.gz", Code = "cat", LangName = "Catalan", Size = 1.6 },
            new Language { Filename = "tesseract-ocr-3.02.bul.tar.gz", Code = "bul", LangName = "Bulgarian", Size = 1.5 },
            new Language { Filename = "tesseract-ocr-3.02.ben.tar.gz", Code = "ben", LangName = "Bengali", Size = 6.5 },
            new Language { Filename = "tesseract-ocr-3.02.bel.tar.gz", Code = "bel", LangName = "Belarusian", Size = 1.2 },
            new Language { Filename = "tesseract-ocr-3.02.aze.tar.gz", Code = "aze", LangName = "Azerbaijani", Size = 1.4 },
            new Language { Filename = "tesseract-ocr-3.02.ara.tar.gz", Code = "ara", LangName = "Arabic", Size = 6.3 },
            new Language { Filename = "tesseract-ocr-3.02.afr.tar.gz", Code = "afr", LangName = "Afrikaans", Size = 1.0 },
            new Language { Filename = "tesseract-ocr-3.02.jpn.tar.gz", Code = "jpn", LangName = "Japanese", Size = 13.1 },
            new Language { Filename = "tesseract-ocr-3.02.chi_sim.tar.gz", Code = "chi_sim", LangName = "Chinese (Simplified)", Size = 17.1 },
            new Language { Filename = "tesseract-ocr-3.02.chi_tra.tar.gz", Code = "chi_tra", LangName = "Chinese (Traditional)", Size = 23.8 },
        };

        private class Language
        {
            public string Filename { get; set; }

            public string Code { get; set; }

            public string LangName { get; set; }

            public double Size { get; set; }
        }
    }
}
