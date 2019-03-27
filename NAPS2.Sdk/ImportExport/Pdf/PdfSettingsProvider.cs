using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    // TODO: Maybe this can be replaced with ConfigProvider<T>? Somehow get individual property locks to work on PdfSettings properties (after ensuring they're all nullable)
    public abstract class PdfSettingsProvider
    {
        private static PdfSettingsProvider _default = Wrap(new PdfSettings());

        public static PdfSettingsProvider Wrap(PdfSettings pdfSettings) => new Wrapper(pdfSettings);

        public static PdfSettingsProvider Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract PdfSettings PdfSettings { get; }

        private class Wrapper : PdfSettingsProvider
        {
            public Wrapper(PdfSettings pdfSettings)
            {
                PdfSettings = pdfSettings;
            }

            public override PdfSettings PdfSettings { get; }
        }
    }
}
