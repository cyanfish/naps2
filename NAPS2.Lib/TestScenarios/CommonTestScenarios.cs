#if TEST_SCENARIOS
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Update;

namespace NAPS2.TestScenarios;

/// <summary>
/// Test scenarios that work on any platform (where the UI exists).
/// </summary>
internal class CommonTestScenarios : TestScenarioProvider
{
    protected override void Define()
    {
        DefineMessages();
        DefineErrors();
        DefineProgress();
        DefineDesktop();
        DefineNotifications();
        DefineForms();
        DefineDevices();
        DefineFileDialogs();
    }

    private void DefineMessages()
    {
        Add("msg-reset-images", async () =>
        {
            LoadImages();
            await ShowMessage([K(MiscResources.ConfirmResetImages), K(MiscResources.ResetImage)],
                string.Format(MiscResources.ConfirmResetImages, 1), MiscResources.ResetImage,
                MessageBoxButtons.OKCancel, MessageBoxType.Question);
        });
        Add("msg-exit-active-operations", () => ShowMessage(
            [K(MiscResources.ExitWithActiveOperations), K(MiscResources.ActiveOperations)],
            MiscResources.ExitWithActiveOperations, MiscResources.ActiveOperations,
            MessageBoxButtons.YesNo, MessageBoxType.Warning));
        Add("msg-exit-unsaved-changes", () => ShowMessage(
            [K(MiscResources.ExitWithUnsavedChanges), K(MiscResources.UnsavedChanges)],
            MiscResources.ExitWithUnsavedChanges, MiscResources.UnsavedChanges,
            MessageBoxButtons.YesNo, MessageBoxType.Warning));
        Add("msg-confirm-cancel-batch", () => ShowMessage(
            [K(MiscResources.ConfirmCancelBatch), K(MiscResources.CancelBatch)],
            MiscResources.ConfirmCancelBatch, MiscResources.CancelBatch,
            MessageBoxButtons.YesNo, MessageBoxType.Warning));
        Add("msg-confirm-delete-page-size", () => ShowMessage(
            [K(MiscResources.ConfirmDelete)],
            string.Format(MiscResources.ConfirmDelete, "Receipt"), MiscResources.Delete,
            MessageBoxButtons.OKCancel, MessageBoxType.Warning));
        Add("msg-confirm-delete-profile", () => ShowMessage(
            [K(MiscResources.ConfirmDeleteSingleProfile)],
            string.Format(MiscResources.ConfirmDeleteSingleProfile, "My Scanner"), MiscResources.Delete,
            MessageBoxButtons.OKCancel, MessageBoxType.Warning));
        Add("msg-select-profile", () => ShowMessage(
            [K(MiscResources.SelectProfileBeforeScan), K(MiscResources.ChooseProfile)],
            MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile));
        Add("msg-download-error", () => ShowMessage(
            [K(MiscResources.FilesCouldNotBeDownloaded), K(MiscResources.DownloadError)],
            MiscResources.FilesCouldNotBeDownloaded, MiscResources.DownloadError,
            MessageBoxButtons.OK, MessageBoxType.Error));
    }

    private void DefineErrors()
    {
        void Simple(string name, string message, [CallerArgumentExpression(nameof(message))] string key = "") =>
            Add("error-" + name, () => ShowError([key], message));

        void WithDetails(string name, string message, string details,
            [CallerArgumentExpression(nameof(message))] string key = "") =>
            Add("error-" + name, () => ShowError([key], message, details));

        Simple("name-missing", MiscResources.NameMissing);
        Simple("no-device-selected", MiscResources.NoDeviceSelected);
        Simple("device-not-found", SdkResources.DeviceNotFound);
        Simple("update-error", MiscResources.UpdateError);
        Simple("no-pages-in-feeder", SdkResources.NoPagesInFeeder);
        Simple("no-devices-found", SdkResources.NoDevicesFound);
        Simple("paper-jam", SdkResources.DevicePaperJam);
        Simple("warming-up", SdkResources.DeviceWarmingUp);
        Simple("cover-open", SdkResources.DeviceCoverOpen);
        Simple("driver-not-supported", SdkResources.DriverNotSupported);
        Simple("no-feeder-support", SdkResources.NoFeederSupport);
        Simple("no-duplex-support", SdkResources.NoDuplexSupport);
        Simple("device-busy", SdkResources.DeviceBusy);
        Simple("device-offline", SdkResources.DeviceOffline);
        Simple("device-communication", SdkResources.DeviceCommunicationFailure);
        Simple("worker-crash", SdkResources.WorkerCrash);
        Simple("worker-crash-windows", SdkResources.WorkerCrashWindows);
        Simple("ocr-timeout", SdkResources.OcrTimeout);

        const string details = "System.Exception: Example error details";
        WithDetails("unknown-driver", SdkResources.UnknownDriverError,
            "NAPS2.Scan.Exceptions.ScanDriverUnknownException: WIA error code: 80210015");
        WithDetails("auth", MiscResources.AuthError, details);
        WithDetails("auto-save", MiscResources.AutoSaveError, details);
        WithDetails("saving", MiscResources.ErrorSaving, details);
        WithDetails("emailing", MiscResources.ErrorEmailing, details);
        WithDetails("email", MiscResources.EmailError, "MAPI returned error code: 2");
        WithDetails("ocr", SdkResources.OcrError, details);
        WithDetails("batch", MiscResources.BatchError, details);
        WithDetails("connection", UiStrings.ConnectionError, details);
        WithDetails("file-in-use", MiscResources.FileInUse,
            "System.IO.IOException: The process cannot access the file because it is being used by another process.");
        WithDetails("no-permission", MiscResources.DontHavePermission,
            "System.UnauthorizedAccessException: Access to the path is denied.");
        Add("error-file-in-use-retry", async () =>
        {
            var window = await Ctx.WaitForNewWindow(() => _ = Get<ErrorOutput>().DisplayErrorWithRetry(
                MiscResources.FileInUse,
                new IOException("The process cannot access the file because it is being used by another process.")));
            await Ready([K(MiscResources.FileInUse), K(UiStrings.Retry)], window.Title);
        });
        Add("error-starting-application", () => ShowError([K(UiStrings.ErrorStartingApplication)],
            string.Format(UiStrings.ErrorStartingApplication, @"C:\Program Files\Example\example.exe"), details));
        Add("error-import-file", () => ShowError([K(MiscResources.ImportErrorCouldNot)],
            string.Format(MiscResources.ImportErrorCouldNot, "document.pdf"), details));
    }

    private void DefineProgress()
    {
        Add("progress-ocr", () => ShowProgress([K(MiscResources.OcrProgress), K(MiscResources.RunningOcr)],
            MiscResources.OcrProgress, MiscResources.RunningOcr, 2, 5));
        Add("progress-copy", () => ShowProgress([K(MiscResources.CopyProgress), K(MiscResources.Copying)],
            MiscResources.CopyProgress, MiscResources.Copying, 3, 8));
        Add("progress-direct-import", () => ShowProgress([K(MiscResources.Importing)],
            MiscResources.ImportProgress, MiscResources.Importing, 3, 8));
        Add("progress-import-file", () => ShowProgress([K(MiscResources.ImportingFormat)],
            MiscResources.ImportProgress, string.Format(MiscResources.ImportingFormat, "document.pdf"), 1, 4));
        Add("progress-deskew", () => ShowProgress(
            [K(MiscResources.AutoDeskewProgress), K(MiscResources.AutoDeskewing)],
            MiscResources.AutoDeskewProgress, MiscResources.AutoDeskewing, 2, 6));
        Add("progress-email-pdf", () => ShowProgress(
            [K(MiscResources.EmailPdfProgress), K(MiscResources.UploadingEmail)],
            MiscResources.EmailPdfProgress, MiscResources.UploadingEmail));
        Add("progress-recovery", () => ShowProgress(
            [K(MiscResources.RecoveryProgress), K(MiscResources.Recovering)],
            MiscResources.RecoveryProgress, MiscResources.Recovering, 4, 12));
        Add("progress-save-images", () => ShowProgress(
            [K(MiscResources.SaveImagesProgress), K(MiscResources.SavingFormat)],
            MiscResources.SaveImagesProgress, string.Format(MiscResources.SavingFormat, "scan_2.jpg"), 2, 4));
        Add("progress-save-pdf", () => ShowProgress(
            [K(MiscResources.SavePdfProgress), K(MiscResources.SavingFormat)],
            MiscResources.SavePdfProgress, string.Format(MiscResources.SavingFormat, "scan.pdf"), 2, 4));
        Add("progress-update", () => ShowProgress(
            [K(MiscResources.UpdateProgress), K(MiscResources.Updating), K(MiscResources.SizeProgress)],
            MiscResources.UpdateProgress, MiscResources.Updating, 4_200_000, 52_400_000,
            OperationProgressType.MB));
        Add("progress-scan-acquiring", () => ShowProgress([K(MiscResources.AcquiringData)],
            "Canon CanoScan LiDE 400", MiscResources.AcquiringData, allowBackground: false));
        Add("progress-scan-page", () => ShowProgress([K(MiscResources.ScanProgressPage)],
            "Canon CanoScan LiDE 400", string.Format(MiscResources.ScanProgressPage, 2), allowBackground: false));
    }

    private void DefineDesktop()
    {
        Add("desktop-scanned", async () =>
        {
            AddProfile();
            Ctx.RunCommand(c => c.Scan);
            await Ctx.WaitUntil(() => Ctx.Ui(() => Get<UiImageList>().Images.Count == 2), "2 scanned images");
            await Ready([K(UiStrings.Scan)], DesktopTitle);
        }, setup => setup.FakeScanImages = [null, null]);
        Add("desktop-images", async () =>
        {
            LoadImages();
            await Ready([K(UiStrings.ToggleSidebar), K(UiStrings.ZoomIn), K(UiStrings.ZoomOut), K(UiStrings.NewProfile)],
                DesktopTitle);
        });
        Add("menu-tray", async () =>
        {
            var tray = Ctx.Ui(() => new ServerTrayIndicator());
            Ctx.Dispatch(() => tray.Menu.Show(Desktop, new PointF(500, 300)));
            await Ready([K(UiStrings.StopScannerSharing)], DesktopTitle, popupCrop: 20);
        });

        void Shortcuts(string name, bool bottom)
        {
            string[] top =
            [
                K(UiStrings.SaveAllAsPdf), K(UiStrings.SaveSelectedAsPdf), K(UiStrings.SaveAllAsImages),
                K(UiStrings.SaveSelectedAsImages), K(UiStrings.EmailAllAsPdf), K(UiStrings.EmailSelectedAsPdf),
                K(UiStrings.EditWith)
            ];
            string[] end =
            [
                K(UiStrings.EditWith), K(UiStrings.ManualDuplexPreview), K(UiStrings.ReverseAll),
                K(UiStrings.ReverseSelected), K(UiStrings.ZoomIn), K(UiStrings.ZoomOut)
            ];
            Add(name, () => ShowForm<CombinedSettingsForm>(bottom ? end : top,
                f => f.SelectPanel<KeyboardShortcutsPanel>(),
                before: f =>
                {
                    // Make the form as tall as the screen to show as many shortcuts as possible
                    var workingArea = f.Screen.WorkingArea;
                    f.Bounds = new Rectangle(f.Location.X, (int) workingArea.Top + 10, f.Width,
                        (int) workingArea.Height - 20);
                    if (bottom)
                    {
                        var grid = TestScenarioUi.GetControls(f).OfType<GridView>().First();
                        grid.ScrollToRow(grid.DataStore.Count() - 1);
                    }
                }));
        }
        Shortcuts("keyboard-shortcuts-top", false);
        Shortcuts("keyboard-shortcuts-bottom", true);
    }

    private void DefineNotifications()
    {
        void Notify(string name, string[] strings, Action<INotify> action) => Add("notify-" + name, async () =>
        {
            LoadImages();
            Ctx.Ui(() => action(Get<INotify>()));
            await Ready(strings, DesktopTitle);
        });

        Notify("pdf-saved", [K(MiscResources.PdfSaved)], n => n.PdfSaved(@"C:\Users\Example\Documents\scan.pdf"));
        Notify("image-saved", [K(MiscResources.ImageSaved)],
            n => n.ImagesSaved(1, @"C:\Users\Example\Pictures\scan.jpg"));
        Notify("images-saved", [K(MiscResources.ImagesSaved)],
            n => n.ImagesSaved(4, @"C:\Users\Example\Pictures\scan_1.jpg"));
        Notify("donate", [K(MiscResources.DonatePrompt)], n => n.DonatePrompt());
        Notify("review", [K(MiscResources.ReviewPrompt), K(MiscResources.LeaveAReview)], n => n.ReviewPrompt());
        Notify("update", [K(MiscResources.UpdateAvailable), K(MiscResources.Install)],
            n => n.UpdateAvailable(Get<IUpdateChecker>(), new UpdateInfo("NAPS2 8.3.0", "", [], [])));
        Notify("pages-reordered", [K(UiStrings.PagesReordered)], n => n.PagesReordered());
        Add("notify-open-folder", async () =>
        {
            LoadImages();
            Ctx.Ui(() => Get<INotify>().PdfSaved(@"C:\Users\Example\Documents\scan.pdf"));
            await Ready([K(UiStrings.OpenFolder)], DesktopTitle, popupCrop: 100, before: () =>
            {
                // Show the link's right-click menu (below the link, as by default it's at the mouse position)
                Desktop.NotificationArea.Views.OfType<LinkNotificationView>().First().ShowContextMenu();
            });
        });
    }

    private void DefineForms()
    {
        Add("about-checking", () => ShowForm<AboutForm>(
                [K(UiStrings.EnableDebugLogging), K(MiscResources.CheckingForUpdates)]),
            setup =>
            {
                setup.Config(c => c.CheckForUpdates, true);
                setup.FakeUpdateCheckNeverCompletes = true;
            });
        Add("about-no-updates", () => ShowForm<AboutForm>([K(MiscResources.NoUpdates)]),
            setup => setup.Config(c => c.CheckForUpdates, true));
        Add("about-update", () => ShowForm<AboutForm>([K(MiscResources.Install)]),
            setup =>
            {
                setup.Config(c => c.CheckForUpdates, true);
                setup.FakeUpdate = new UpdateInfo("NAPS2 8.3.0", "", [], []);
            });
        Add("settings", () => ShowForm<CombinedSettingsForm>([K(UiStrings.KeepSession)]));
        Add("toolbar-settings", () => ShowForm<CombinedSettingsForm>(
            [
                K(UiStrings.ToolbarSettings), K(UiStrings.GeneralSettings), K(UiStrings.Toolbar),
                K(UiStrings.PositionLabel), K(UiStrings.ButtonsLabel), K(UiStrings.MoveUpDown),
                K(UiStrings.ProfilesToolbar)
            ],
            f => f.SelectPanel<ToolbarSettingsPanel>()));
        // The profiles toolbar's position dropdown is disabled, so this opens the main toolbar's
        Add("toolbar-settings-position", () => ShowForm<CombinedSettingsForm>(
            [K(SettingsResources.DockStyle_Top), K(SettingsResources.DockStyle_Bottom)],
            f => f.SelectPanel<ToolbarSettingsPanel>(),
            before: _ => OpenDropDown(SettingsResources.DockStyle_Top)));

        void ManualIp(string name, string[] strings, Action<ManualIpForm>? setState = null) =>
            Add(name, () => ShowForm<ManualIpForm>(strings, before: f =>
            {
                TestScenarioUi.GetControls(f).OfType<TextBox>().First().Text = "192.168.1.25";
                setState?.Invoke(f);
            }));
        ManualIp("manual-ip-waiting", [K(UiStrings.WaitingToConnect)]);
        ManualIp("manual-ip-connecting", [K(UiStrings.Connecting)], f => f.SetConnecting());
        ManualIp("manual-ip-timeout", [K(UiStrings.ConnectionTimedOut)], f => f.SetError(UiStrings.ConnectionTimedOut));

        Add("ocr-setup", () =>
        {
            // Copy the installed OCR languages from the user's real NAPS2 data so the form can load
            FileSystemHelper.CopyDirectory(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NAPS2", "components", "tesseract4"),
                Path.Combine(Paths.Components, "tesseract4"), true);
            return ShowForm<OcrSetupForm>([K(UiStrings.PreemptivelyOcrAfterScanning)]);
        });
        Add("scanner-sharing-service", () => ShowForm<ScannerSharingForm>([K(UiStrings.ShareAsService)]));
        Add("authorize", () => ShowForm<AuthorizeForm>(
                [K(UiStrings.AuthorizeFormTitle), K(UiStrings.WaitingForAuthorization)]),
            _ => TestScenarioFlags.SkipFormWork = true);
        Add("download-progress", () => ShowForm<DownloadProgressForm>(
                [K(UiStrings.DownloadProgressFormTitle), K(MiscResources.FilesProgressFormat), K(MiscResources.SizeProgress)],
                before: f =>
                {
                    f.UpdateProgress(1, 3, 4_200_000, 10_500_000);
                }),
            _ => TestScenarioFlags.SkipFormWork = true);
        Add("ocr-download-languages", () => ShowForm<OcrDownloadForm>(
            [K(UiStrings.OcrDownloadSummaryText), K(UiStrings.OcrSelectLanguageLabel)]));
        Add("pdf-password", () => ShowForm<PdfPasswordForm>([K(UiStrings.EncryptedFilePrompt)],
            f => f.FileName = "contract.pdf"));
        Add("edit-with", () => ShowForm<EditWithForm>([K(UiStrings.EditWithFormTitle)]),
            setup => setup.FakeOpenWith =
            [
                new OpenWithEntry("paint", "Paint", "", 0),
                new OpenWithEntry("photos", "Photos", "", 0),
                new OpenWithEntry("gimp", "GIMP 2.10", "", 0)
            ]);
        Add("placeholders-counter", () => ShowForm<PlaceholdersForm>([K(UiStrings.AutoIncrementing1Digit)],
            f => f.FileName = "$(YYYY)-$(MM)-$(DD) $(n)"));
        Add("pdf-settings-compat", () => ShowForm<CombinedSettingsForm>(
            [
                K(SettingsResources.PdfCompat_PdfA1B), K(SettingsResources.PdfCompat_PdfA2B),
                K(SettingsResources.PdfCompat_PdfA3B), K(SettingsResources.PdfCompat_PdfA3U)
            ],
            f => f.SelectPanel<PdfSettingsPanel>(),
            before: _ => OpenDropDown(SettingsResources.PdfCompat_PdfA1B)));
        Add("image-settings-tiff", () => ShowForm<CombinedSettingsForm>(
            [K(SettingsResources.TiffComp_Lzw), K(SettingsResources.TiffComp_Ccitt4), K(SettingsResources.TiffComp_None)],
            f => f.SelectPanel<ImageSettingsPanel>(),
            before: _ => OpenDropDown(SettingsResources.TiffComp_Lzw)));
        Add("manual-duplex", () =>
        {
            LoadImages(4, 4);
            return ShowForm<ManualDuplexForm>([K(UiStrings.Preview)]);
        });
        Add("crop-help", () =>
        {
            var images = LoadImages();
            return ShowForm<CropForm>(
                [K(UiStrings.AutoCrop), K(UiStrings.ControlKey), K(UiStrings.AltKey), K(UiStrings.ShiftKey)],
                f => SetImage(f, images[0]),
                before: f => TestScenarioUi.Click(TestScenarioUi.FindControl(f, UiStrings.Help, null, null)));
        });
        Add("profiles-context-menu", () =>
        {
            AddProfile();
            var second = AddProfile("Feeder Duplex", deviceName: "Brother ADS-1700W");
            return ShowForm<ProfilesForm>([K(UiStrings.SetDefault)], popupCrop: 60, before: f =>
            {
                f.ShowContextMenu(second, new PointF(150, 40));
            });
        });
        Add("edit-profile-autosave", () => ShowForm<EditProfileForm>([K(UiStrings.EnableAutoSave)],
            f => f.ScanProfile = NewProfile()));
        Add("edit-profile-page-size", () =>
        {
            var profile = NewProfile();
            profile.PageSize = ScanPageSize.Custom;
            profile.CustomPageSize = new PageDimensions { Width = 5, Height = 7, Unit = LocalizedPageSizeUnit.Inch };
            return ShowForm<EditProfileForm>([K(MiscResources.NamedPageSizeFormat), K(MiscResources.CustomPageSizeFormat)],
                f => f.ScanProfile = profile,
                before: _ => Ctx.OpenDropDown(item => item.StartsWith("Receipt (")));
        }, setup => setup.Config(c => c.CustomPageSizePresets, ImmutableList.Create(
            new NamedPageSize
            {
                Name = "Receipt",
                Dimens = new PageDimensions { Width = 80, Height = 200, Unit = LocalizedPageSizeUnit.Millimetre }
            })));
        Add("advanced-profile", () =>
        {
            var profile = AddProfile();
            return ShowForm<AdvancedProfileForm>(
                [K(UiStrings.AutoCropScannedPages), K(UiStrings.AppleImplLabel), K(SettingsResources.AppleImpl_FileTransfer)],
                f => f.ScanProfile = profile,
                before: _ => OpenDropDown(SettingsResources.AppleImpl_FileTransfer));
        }, _ => TestScenarioFlags.ShowAllDrivers = true);

        void Batch(string name, string status, bool done, [CallerArgumentExpression(nameof(status))] string key = "")
        {
            Add("batch-" + name, () =>
            {
                AddProfile();
                return ShowForm<BatchScanForm>(done ? [key, K(MiscResources.Close)] : [key],
                    f => f.SimulateStatus(status, done));
            });
        }
        Batch("cancelled", MiscResources.BatchStatusCancelled, true);
        Batch("complete", MiscResources.BatchStatusComplete, true);
        Batch("error", MiscResources.BatchStatusError, true);
        Batch("cancelling", MiscResources.BatchStatusCancelling, false);
        Batch("saving", MiscResources.BatchStatusSaving, false);
        Batch("scan-page", string.Format(MiscResources.BatchStatusScanPage, 3, 2), false,
            K(MiscResources.BatchStatusScanPage));
        Batch("page", string.Format(MiscResources.BatchStatusPage, 3), false, K(MiscResources.BatchStatusPage));
        Batch("waiting", string.Format(MiscResources.BatchStatusWaitingForScan, 2), false,
            K(MiscResources.BatchStatusWaitingForScan));
        Add("batch-missing-extension", () =>
        {
            AddProfile();
            return ShowForm<BatchScanForm>([K(UiStrings.MissingFileExtension)]);
        }, setup =>
        {
            setup.Config(c => c.BatchSettings.OutputType, BatchOutputType.SingleFile);
            setup.Config(c => c.BatchSettings.SavePath, @"C:\Scans\Scan $(YYYY)-$(MM)-$(DD)");
        });
    }

    private void DefineDevices()
    {
        void ChooseDevice(string name, List<ScanDevice> devices, bool flatpakLink, string[] strings)
        {
            Add("choose-device-" + name, () => ShowForm<ChooseDeviceForm>(strings,
                    f => f.ScanOptions = new ScanOptions { Driver = PlatformDriver },
                    positions: f =>
                    {
                        // Tag the title bar
                        var bounds = Ctx.FindWindow(f.Title).Bounds;
                        return new() { [K(UiStrings.SelectSource)] = bounds with { Height = 32 } };
                    }),
                setup =>
                {
                    TestScenarioFlags.ShowAllDrivers = true;
                    TestScenarioFlags.ShowFlatpakLink = flatpakLink;
                    setup.FakeDevices = devices;
                });
        }
        ChooseDevice("list", ExampleDevices, true,
            [
                K(UiStrings.SaneDriver), K(UiStrings.AppleDriver), K(UiStrings.SelectSource),
                K(UiStrings.DevicesFound), K(UiStrings.CantFindScannerFlatpak)
            ]);
        ChooseDevice("single", [new ScanDevice(PlatformDriver, "unknown", SdkResources.UnknownScanner)], false,
            [K(UiStrings.DeviceFoundSingular), K(SdkResources.UnknownScanner)]);
        ChooseDevice("none", [], false, [K(UiStrings.NoDevicesFound)]);
    }

    private void DefineFileDialogs()
    {
        Add("file-dialog-save-images", () => ShowFileDialog(
            [
                K(MiscResources.FileTypeBmp), K(MiscResources.FileTypeJpeg), K(MiscResources.FileTypeJp2),
                K(MiscResources.FileTypePng), K(MiscResources.FileTypeTiff)
            ], FileFilterGroup.Image));
        Add("file-dialog-save-pdf", () => ShowFileDialog([K(MiscResources.FileTypePdf)],
            FileFilterGroup.Pdf | FileFilterGroup.Image));
        Add("file-dialog-import", () => ShowFileDialog(
            [K(MiscResources.FileTypeAllFiles), K(MiscResources.FileTypeImageFiles), K(MiscResources.FileTypePdf)],
            FileFilterGroup.AllFiles | FileFilterGroup.Pdf | FileFilterGroup.AllImages | FileFilterGroup.Image,
            save: false));
    }

    private void OpenDropDown(string item) => Ctx.OpenDropDown(x => x == item);
}
#endif
