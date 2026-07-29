            PdfSettings = new PdfSettings
            {
                Metadata = new PdfMetadata
                {
                    Title = MiscResources.ScannedImage,
                    Subject = MiscResources.ScannedImage,
                    Author = MiscResources.NAPS2,
                    Creator = MiscResources.NAPS2,
                    Keywords = ""
                },
                Encryption = new PdfEncryption
                {
                    AllowAnnotations = false,
                    AllowContentCopying = false,
                    AllowContentCopyingForAccessibility = false,
                    AllowDocumentAssembly = false,
                    AllowDocumentModification = false,
                    AllowFormFilling = false,
                    AllowFullQualityPrinting = false,
                    AllowPrinting = false,
                    EncryptPdf = false,
                    OwnerPassword = "",
                    UserPassword = ""
                },
                Compat = PdfCompat.Default,
                DefaultFileName = "",
                SkipSavePrompt = false,
                SinglePagePdfs = false,
                UseDefaultFileNamePlaceholder = true
            },
            RememberPdfSettings = false,
            ImageSettings = new ImageSettings
            {
                DefaultFileName = "",
                SkipSavePrompt = false,
                JpegQuality = 75,
                SinglePageTiff = false,
                TiffCompression = TiffCompression.Auto,
                UseDefaultFileNamePlaceholder = true
            },
