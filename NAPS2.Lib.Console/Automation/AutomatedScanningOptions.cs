using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace NAPS2.Automation
{
    public class AutomatedScanningOptions : CommandLineOptions
    {
        #region General Options

        [Option('o', "output", HelpText = "The name and path of the file to save." +
                                          " The extension determines the output type (e.g. .pdf for a PDF file, .jpg for a JPEG)." +
                                          " Placeholders can be used (e.g. $(YYYY)-$(MM)-$(DD) for the date, $(hh)_$(mm)_$(ss) for the time, $(nnnn) for an auto-incrementing number).")]
        public string OutputPath { get; set; }

        [Option('a', "autosave", HelpText = "Use the Auto Save settings from the selected profile." +
                                            " Only works if the profile has Auto Save enabled.")]
        public bool AutoSave { get; set; }

        [Option("install", HelpText = "Use this option to download and install optional components (e.g. \"ocr-eng\", \"generic-import\").")]
        public string Install { get; set; }

        [Option('p', "profile", HelpText = "The name of the profile to use for scanning." +
                                           " If not specified, the most-recently-used profile from the GUI is selected.")]
        public string ProfileName { get; set; }

        [Option('i', "import", HelpText = "The name and path of one or more pdf/image files to import." +
                                          " Imported files are prepended to the output in the order they are specified." +
                                          " Multiple files are separated by a semicolon (\";\")." +
                                          " Slice notation can be used to only import some pages (e.g. \"[0]\" for the first page or \"[:2]\" for the first two pages).")]
        public string ImportPath { get; set; }

        [Option("importpassword", HelpText = "The password to use to import one or more encrypted PDF files.")]
        public string ImportPassword { get; set; }
        
        [Option("progress", HelpText = "Display a graphical window for scanning progress.")]
        public bool Progress { get; set; }

        [Option('v', "verbose", HelpText = "Display progress information in the console." +
                                           " If not specified, no output is displayed if the scan is successful.")]
        public bool Verbose { get; set; }

        [Option('n', "number", DefaultValue = 1, HelpText = "The number of scans to perform.")]
        public int Number { get; set; }

        [Option('d', "delay", DefaultValue = 0, HelpText = "The delay (in milliseconds) between each scan.")]
        public int Delay { get; set; }

        [Option('f', "force", HelpText = "Overwrite existing files." +
                                         " If not specified, any files that already exist will not be changed.")]
        public bool ForceOverwrite { get; set; }

        [Option('w', "wait", HelpText = "After finishing, wait for user input (enter/return) before exiting.")]
        public bool WaitForEnter { get; set; }

        #endregion

        #region Order Options

        [Option("interleave", HelpText = "Interleave pages before saving.")]
        public bool Interleave { get; set; }

        [Option("altinterleave", HelpText = "Alternate Interleave pages before saving.")]
        public bool AltInterleave { get; set; }

        [Option("deinterleave", HelpText = "Deinterleave pages before saving.")]
        public bool Deinterleave { get; set; }

        [Option("altdeinterleave", HelpText = "Alternate Deinterleave pages before saving.")]
        public bool AltDeinterleave { get; set; }

        [Option("reverse", HelpText = "Reverse pages before saving.")]
        public bool Reverse { get; set; }

        #endregion

        #region Split Options

        [Option("split", HelpText = "Split the pages into individual PDF/TIFF files.")]
        public bool Split { get; set; }

        [Option("splitscans", HelpText = "Split the pages into multiple PDF/TIFF files, one for each scan.")]
        public bool SplitScans { get; set; }

        [Option("splitpatcht", HelpText = "Split the pages into multiple PDF/TIFF files, separating by Patch-T.")]
        public bool SplitPatchT { get; set; }

        [Option("splitsize", HelpText = "Split the pages into multiple PDF/TIFF files with the given number of pages per file.")]
        public int SplitSize { get; set; }

        #endregion

        #region PDF Options

        [Option("pdftitle", HelpText = "The title for generated PDF metadata.")]
        public string PdfTitle { get; set; }

        [Option("pdfauthor", HelpText = "The author for generated PDF metadata.")]
        public string PdfAuthor { get; set; }

        [Option("pdfsubject", HelpText = "The subject for generated PDF metadata.")]
        public string PdfSubject { get; set; }

        [Option("pdfkeywords", HelpText = "The keywords for generated PDF metadata.")]
        public string PdfKeywords { get; set; }

        [Option("usesavedmetadata", HelpText = "Use the metadata (title, author, subject, keywords) configured in the GUI, if any, for the generated PDF.")]
        public bool UseSavedMetadata { get; set; }

        [Option("encryptconfig", HelpText = "The name and path of an XML file to configure encryption for the generated PDF.")]
        public string EncryptConfig { get; set; }

        [Option("usesavedencryptconfig", HelpText = "Use the encryption configured in the GUI, if any, for the generated PDF.")]
        public bool UseSavedEncryptConfig { get; set; }

        [Option("pdfcompat", HelpText = "The standard to use for the generated PDF. Possible values: default, A1-b, A2-b, A3-b, A3-u")]
        public string PdfCompat { get; set; }

        #endregion

        #region OCR Options

        [Option("enableocr", HelpText = "Enable OCR for generated PDFs.")]
        public bool EnableOcr { get; set; }

        [Option("disableocr", HelpText = "Disable OCR for generated PDFs. Overrides --enableocr.")]
        public bool DisableOcr { get; set; }

        [Option("ocrlang", HelpText = "The three-letter code for the language used for OCR (e.g. 'eng' for English, 'fra' for French, etc.). Implies --enableocr.")]
        public string OcrLang { get; set; }

        #endregion

        #region Email Options

        [Option('e', "email", HelpText = "The name of the file to attach to an email." +
                                         " The extension determines the output type (e.g. .pdf for a PDF file, .jpg for a JPEG).")]
        //" You can use \"<date>\" and/or \"<time>\" to insert the date/time of the scan.")]
        public string EmailFileName { get; set; }

        [Option("subject", HelpText = "The email message's subject." +
                                      //" You can use \"<date>\" and/or \"<time>\" to insert the date/time of the scan." +
                                      " Requires -e/--email.")]
        public string EmailSubject { get; set; }

        [Option("body", HelpText = "The email message's body text." +
                                   //" You can use \"<date>\" and/or \"<time>\" to insert the date/time of the scan." +
                                   " Requires -e/--email.")]
        public string EmailBody { get; set; }

        [Option("to", HelpText = "A comma-separated list of email addresses of the recipients." +
                                 " Requires -e/--email.")]
        public string EmailTo { get; set; }

        [Option("cc", HelpText = "A comma-separated list of email addresses of the recipients." +
                                 " Requires -e/--email.")]
        public string EmailCc { get; set; }

        [Option("bcc", HelpText = "A comma-separated list of email addresses of the recipients." +
                                  " Requires -e/--email.")]
        public string EmailBcc { get; set; }

        [Option("autosend", HelpText = "Actually send the email immediately after scanning completes without prompting the user for changes." +
                                       " However, this may prompt the user to login. To avoid that, use --silentsend." +
                                       " Note that Outlook may still require user interaction to send an email, regardless of --autosend or --silentsend options." +
                                       " Requires -e/--email.")]
        public bool EmailAutoSend { get; set; }

        [Option("silentsend", HelpText = "Doesn't prompt the user to login when --autosend is specified." +
                                         " This may result in failure if authentication is required." +
                                         " Note that Outlook may still require user interaction to send an email, regardless of --autosend or --silentsend options." +
                                         " Requires --autosend.")]
        public bool EmailSilentSend { get; set; }

        #endregion

        #region Image Options

        [Option("jpegquality", DefaultValue = 75, HelpText = "The quality of saved JPEG files (0-100, default 75).")]
        public int JpegQuality { get; set; }

        [Option("tiffcomp", HelpText = "The compression to use for TIFF files. Possible values: auto, lzw, ccitt4, none")]
        public string TiffComp { get; set; }
        
        #endregion
    }
}
