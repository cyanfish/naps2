using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Scan.Sane;
using NUnit.Framework;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "unit,fast")]
    class SaneOptionParserTests
    {
        [Test]
        public void Parse()
        {
            var parser = new SaneOptionParser();
            var options = parser.Parse(GetStreamReader(TEST_OUTPUT));

            Assert.That(options.Keys, Is.EquivalentTo(new []
            {
                "--resolution",
                "--mode",
                "--source",
                "--button-controlled",
                "--custom-gamma",
                "--gamma-table",
                "--gamma",
                "-l",
                "-t",
                "-x",
                "-y",
                "--button-update",
                "--threshold",
                "--threshold-curve"
            }));
            Assert.That(options["--resolution"].WordList, Is.EquivalentTo(new double[] { 75, 150, 300, 600, 1200 }));
            Assert.That(options["--resolution"].Unit, Is.EqualTo(SaneUnit.Dpi));
            Assert.That(options["--mode"].StringList, Is.EquivalentTo(new [] { "Color", "Gray", "Lineart" }));
            Assert.That(options["--mode"].Capabilities & SaneCapabilities.Automatic, Is.EqualTo(SaneCapabilities.Automatic));
            Assert.That(options["-x"].Type, Is.EqualTo(SaneValueType.Numeric));
            Assert.That(options["-x"].Range.Min, Is.EqualTo(0));
            Assert.That(options["-x"].Range.Max, Is.EqualTo(decimal.Parse("216.069")));
            Assert.That(options["-x"].CurrentNumericValue, Is.EqualTo(decimal.Parse("216.069")));
            Assert.That(options["--source"].Desc, Is.EqualTo("Selects the scan source (such as a document-feeder). Set source before mode and resolution. Resets mode and resolution to auto values."));
            Assert.That(options["--gamma-table"].Type, Is.EqualTo(SaneValueType.Group));
            Assert.That(options["--button-update"].Type, Is.EqualTo(SaneValueType.Button));
        }

        private StreamReader GetStreamReader(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream);
        }

        private const string TEST_OUTPUT = @"Usage: scanimage [OPTION]...

Start image acquisition on a scanner device and write image data to
standard output.

Parameters are separated by a blank from single-character options (e.g.
-d epson) and by a ""="" from multi-character options (e.g. --device-name=epson).
-d, --device-name=DEVICE   use a given scanner device (e.g. hp:/dev/scanner)
    --format=pnm|tiff      file format of output file
-i, --icc-profile=PROFILE  include this ICC profile into TIFF file
-L, --list-devices         show available scanner devices
-f, --formatted-device-list=FORMAT similar to -L, but the FORMAT of the output
                           can be specified: %d (device name), %v (vendor),
                           %m (model), %t (type), %i (index number), and
                           %n (newline)
-b, --batch[=FORMAT]       working in batch mode, FORMAT is `out%d.pnm' or
                           `out%d.tif' by default depending on --format
    --batch-start=#        page number to start naming files with
    --batch-count=#        how many pages to scan in batch mode
    --batch-increment=#    increase page number in filename by #
    --batch-double         increment page number by two, same as
                           --batch-increment=2
    --batch-print          print image filenames to stdout
    --batch-prompt         ask for pressing a key before scanning a page
    --accept-md5-only      only accept authorization requests using md5
-p, --progress             print progress messages
-n, --dont-scan            only set options, don't actually scan
-T, --test                 test backend thoroughly
-A, --all-options          list all available backend options
-h, --help                 display this help message and exit
-v, --verbose              give even more status messages
-B, --buffer-size=#        change input buffer size (in kB, default 32)
-V, --version              print version information

Options specific to device `pixma:23B30942_D10328':
   Scan mode:
    --resolution auto||75|150|300|600|1200dpi [75]
        Sets the resolution of the scanned image.
    --mode auto|Color|Gray|Lineart [Color]
        Selects the scan mode (e.g., lineart, monochrome, or color).
    --source Flatbed [Flatbed]
        Selects the scan source (such as a document-feeder). Set source before
        mode and resolution. Resets mode and resolution to auto values.
    --button-controlled[=(yes|no)] [no]
        When enabled, scan process will not start immediately. To proceed,
        press ""SCAN"" button (for MP150) or ""COLOR"" button (for other models).
        To cancel, press ""GRAY"" button.
  Gamma:
    --custom-gamma[=(auto|yes|no)] [yes]
        Determines whether a builtin or a custom gamma-table should be used.
    --gamma-table auto|0..255,...
        Gamma-correction table.  In color mode this option equally affects the
        red, green, and blue channels simultaneously (i.e., it is an intensity
        gamma table).
    --gamma auto|0.299988..5 [2.2]
        Changes intensity of midtones
  Geometry:
    -l auto|0..216.069mm [0]
        Top-left x position of scan area.
    -t auto|0..297.011mm [0]
        Top-left y position of scan area.
    -x auto|0..216.069mm [216.069]
        Width of scan-area.
    -y auto|0..297.011mm [297.011]
        Height of scan-area.
  Buttons:
    --button-update
        Update button state
  Extras:
    --threshold auto|0..100% (in steps of 1) [inactive]
        Select minimum-brightness to get a white point
    --threshold-curve auto|0..127 (in steps of 1) [inactive]
        Dynamic threshold curve, from light to dark, normally 50-65

Type ``scanimage --help -d DEVICE'' to get list of all options for DEVICE.

List of available devices:
    pixma:23B30942
";
    }
}
