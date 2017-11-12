using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class KeyboardShortcuts
    {
        public string ScanDefault { get; set; }
        public string ScanProfile1 { get; set; }
        public string ScanProfile2 { get; set; }
        public string ScanProfile3 { get; set; }
        public string ScanProfile4 { get; set; }
        public string ScanProfile5 { get; set; }
        public string ScanProfile6 { get; set; }
        public string ScanProfile7 { get; set; }
        public string ScanProfile8 { get; set; }
        public string ScanProfile9 { get; set; }
        public string ScanProfile10 { get; set; }
        public string ScanProfile11 { get; set; }
        public string ScanProfile12 { get; set; }
        public string NewProfile { get; set; }
        public string BatchScan { get; set; }

        public string Profiles { get; set; }

        public string Ocr { get; set; }

        public string Import { get; set; }

        public string SavePDF { get; set; }
        public string SavePDFAll { get; set; }
        public string SavePDFSelected { get; set; }

        public string SaveImages { get; set; }
        public string SaveImagesAll { get; set; }
        public string SaveImagesSelected { get; set; }

        public string EmailPDF { get; set; }
        public string EmailPDFAll { get; set; }
        public string EmailPDFSelected { get; set; }

        public string Print { get; set; }

        public string ImageView { get; set; }
        public string ImageBlackWhite { get; set; }
        public string ImageBrightness { get; set; }
        public string ImageContrast { get; set; }
        public string ImageCrop { get; set; }
        public string ImageHue { get; set; }
        public string ImageSaturation { get; set; }
        public string ImageSharpen { get; set; }
        public string ImageReset { get; set; }

        public string RotateLeft { get; set; }
        public string RotateRight { get; set; }
        public string RotateFlip { get; set; }
        public string RotateCustom { get; set; }

        public string MoveUp { get; set; }
        public string MoveDown { get; set; }

        public string ReorderInterleave { get; set; }
        public string ReorderDeinterleave { get; set; }
        public string ReorderAltInterleave { get; set; }
        public string ReorderAltDeinterleave { get; set; }
        public string ReorderReverseAll { get; set; }
        public string ReorderReverseSelected { get; set; }

        public string Delete { get; set; }

        public string Clear { get; set; }

        public string About { get; set; }

        public string ZoomIn { get; set; }
        public string ZoomOut { get; set; }
    }
}