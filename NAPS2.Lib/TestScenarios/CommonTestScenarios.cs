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
            await ShowMessage([K(UiStrings.ConfirmResetImages), K(UiStrings.ResetImage), K(UiStrings.Reset)],
                new()
                {
                    Title = UiStrings.ResetImage,
                    Text = string.Format(UiStrings.ConfirmResetImages, 1),
                    Type = MessageBoxType.Question,
                    PrimaryText = UiStrings.Reset
                });
        });
        Add("msg-exit-active-operations", () => ShowMessage(
            [K(UiStrings.ExitWithActiveOperations), K(UiStrings.ActiveOperations), K(UiStrings.Exit)],
            new()
            {
                Title = UiStrings.ActiveOperations,
                Text = UiStrings.ExitWithActiveOperations,
                Type = MessageBoxType.Warning,
                PrimaryText = UiStrings.Exit,
                DefaultButton = MessageDialogResult.Close,
                Destructive = true
            }));
        Add("msg-exit-unsaved-changes", () => ShowMessage(
            [K(UiStrings.ExitWithUnsavedChanges), K(UiStrings.UnsavedChanges), K(UiStrings.DiscardAndExit)],
            new()
            {
                Title = UiStrings.UnsavedChanges,
                Text = UiStrings.ExitWithUnsavedChanges,
                Type = MessageBoxType.Warning,
                PrimaryText = UiStrings.DiscardAndExit,
                DefaultButton = MessageDialogResult.Close,
                Destructive = true
            }), variant: true);
        Add("msg-confirm-cancel-batch", () => ShowMessage(
            [K(UiStrings.ConfirmCancelBatch), K(UiStrings.BatchScanFormTitle), K(UiStrings.CancelBatch),
                K(UiStrings.KeepScanning)],
            new()
            {
                Title = UiStrings.BatchScanFormTitle,
                Text = UiStrings.ConfirmCancelBatch,
                Type = MessageBoxType.Question,
                PrimaryText = UiStrings.CancelBatch,
                CloseText = UiStrings.KeepScanning
            }));
        Add("msg-confirm-overwrite", () => ShowMessage(
            [K(UiStrings.ConfirmOverwriteFile), K(UiStrings.OverwriteFile), K(UiStrings.Overwrite),
                K(UiStrings.Skip)],
            new()
            {
                Title = UiStrings.OverwriteFile,
                Text = string.Format(UiStrings.ConfirmOverwriteFile, "scan.pdf"),
                Type = MessageBoxType.Warning,
                PrimaryText = UiStrings.Overwrite,
                SecondaryText = UiStrings.Skip
            }));
        Add("msg-confirm-delete-page-size", () => ShowMessage(
            [K(UiStrings.ConfirmDelete), K(UiStrings.Delete)],
            new()
            {
                Title = UiStrings.Delete,
                Text = string.Format(UiStrings.ConfirmDelete, "Receipt"),
                Type = MessageBoxType.Question,
                PrimaryText = UiStrings.Delete,
                Destructive = true
            }), variant: true);
        Add("msg-confirm-stop-sharing", () => ShowMessage(
            [K(UiStrings.ConfirmDeleteSharedDevice), K(UiStrings.StopSharing)],
            new()
            {
                Title = UiStrings.StopSharing,
                Text = string.Format(UiStrings.ConfirmDeleteSharedDevice, "Canon CanoScan LiDE 400"),
                Type = MessageBoxType.Warning,
                PrimaryText = UiStrings.StopSharing,
                Destructive = true
            }), variant: true);
        Add("msg-confirm-delete-profile", () => ShowMessage(
            [K(UiStrings.ConfirmDeleteSingleProfile), K(UiStrings.Delete)],
            new()
            {
                Title = UiStrings.Delete,
                Text = string.Format(UiStrings.ConfirmDeleteSingleProfile, "My Scanner"),
                Type = MessageBoxType.Warning,
                PrimaryText = UiStrings.Delete,
                Destructive = true
            }));
        Add("msg-select-profile", () => ShowMessage(
            [K(UiStrings.SelectProfileBeforeScan), K(UiStrings.ChooseProfile)],
            new()
            {
                Title = UiStrings.ChooseProfile,
                Text = UiStrings.SelectProfileBeforeScan,
                Type = MessageBoxType.Warning,
                CloseText = UiStrings.OK
            }));
        Add("msg-download-error", () => ShowMessage(
            [K(UiStrings.FilesCouldNotBeDownloaded), K(UiStrings.DownloadError)],
            new()
            {
                Title = UiStrings.DownloadError,
                Text = UiStrings.FilesCouldNotBeDownloaded,
                Type = MessageBoxType.Error,
                CloseText = UiStrings.OK
            }));
    }

    private void DefineErrors()
    {
        void Simple(string name, string message, bool variant = true,
            [CallerArgumentExpression(nameof(message))] string key = "") =>
            Add("error-" + name, () => ShowError([key], message), variant: variant);

        void WithDetails(string name, string message, string details, bool variant = true,
            [CallerArgumentExpression(nameof(message))] string key = "") =>
            Add("error-" + name, () => ShowError([key], message, details), variant: variant);

        Simple("name-missing", UiStrings.NameMissing, variant: false);
        Simple("no-device-selected", UiStrings.NoDeviceSelected);
        Simple("device-not-found", SdkResources.DeviceNotFound);
        Simple("update-error", UiStrings.UpdateError);
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
        Simple("worker-crash-windows", SdkResources.WorkerCrashWindows, variant: false);
        Simple("ocr-timeout", SdkResources.OcrTimeout);

        const string details = "System.Exception: Example error details";
        WithDetails("unknown-driver", SdkResources.UnknownDriverError,
            "NAPS2.Scan.Exceptions.ScanDriverUnknownException: WIA error code: 80210015", variant: false);
        WithDetails("auth", UiStrings.AuthError, details);
        WithDetails("auto-save", UiStrings.AutoSaveError, details);
        WithDetails("saving", UiStrings.ErrorSaving, details);
        WithDetails("emailing", UiStrings.ErrorEmailing, details);
        WithDetails("email", UiStrings.EmailError, "MAPI returned error code: 2");
        WithDetails("ocr", SdkResources.OcrError, details);
        WithDetails("batch", UiStrings.BatchError, details);
        WithDetails("connection", UiStrings.ConnectionError, details);
        WithDetails("file-in-use", UiStrings.FileInUse,
            "System.IO.IOException: The process cannot access the file because it is being used by another process.");
        WithDetails("no-permission", UiStrings.DontHavePermission,
            "System.UnauthorizedAccessException: Access to the path is denied.");
        Add("error-file-in-use-retry", async () =>
        {
            var window = await Ctx.WaitForNewWindow(() => _ = Get<ErrorOutput>().DisplayErrorWithRetry(
                UiStrings.FileInUse,
                new IOException("The process cannot access the file because it is being used by another process.")));
            await Ready([K(UiStrings.FileInUse), K(UiStrings.Retry)], window.Title);
        });
        Add("error-starting-application", () => ShowError([K(UiStrings.ErrorStartingApplication)],
            string.Format(UiStrings.ErrorStartingApplication, @"C:\Program Files\Example\example.exe"), details),
            variant: true);
        Add("error-import-file", () => ShowError([K(UiStrings.ImportErrorCouldNot)],
            string.Format(UiStrings.ImportErrorCouldNot, "document.pdf"), details), variant: true);
    }

    private void DefineProgress()
    {
        Add("progress-ocr", () => ShowProgress([K(UiStrings.OcrProgress), K(UiStrings.RunningOcr)],
            UiStrings.OcrProgress, UiStrings.RunningOcr, 2, 5));
        Add("progress-copy", () => ShowProgress([K(UiStrings.CopyProgress), K(UiStrings.Copying)],
            UiStrings.CopyProgress, UiStrings.Copying, 3, 8), variant: true);
        Add("progress-direct-import", () => ShowProgress([K(UiStrings.Importing)],
            UiStrings.ImportProgress, UiStrings.Importing, 3, 8), variant: true);
        Add("progress-import-file", () => ShowProgress([K(UiStrings.ImportingFormat)],
            UiStrings.ImportProgress, string.Format(UiStrings.ImportingFormat, "document.pdf"), 1, 4),
            variant: true);
        Add("progress-deskew", () => ShowProgress(
            [K(UiStrings.AutoDeskewProgress), K(UiStrings.AutoDeskewing)],
            UiStrings.AutoDeskewProgress, UiStrings.AutoDeskewing, 2, 6), variant: true);
        Add("progress-email-pdf", () => ShowProgress(
            [K(UiStrings.EmailPdfProgress), K(UiStrings.UploadingEmail)],
            UiStrings.EmailPdfProgress, UiStrings.UploadingEmail));
        Add("progress-recovery", () => ShowProgress(
            [K(UiStrings.RecoveryProgress), K(UiStrings.Recovering)],
            UiStrings.RecoveryProgress, UiStrings.Recovering, 4, 12), variant: true);
        Add("progress-save-images", () => ShowProgress(
            [K(UiStrings.SaveImagesProgress), K(UiStrings.SavingFormat)],
            UiStrings.SaveImagesProgress, string.Format(UiStrings.SavingFormat, "scan_2.jpg"), 2, 4),
            variant: true);
        Add("progress-save-pdf", () => ShowProgress(
            [K(UiStrings.SavePdfProgress), K(UiStrings.SavingFormat)],
            UiStrings.SavePdfProgress, string.Format(UiStrings.SavingFormat, "scan.pdf"), 2, 4),
            variant: true);
        Add("progress-update", () => ShowProgress(
            [K(UiStrings.UpdateProgress), K(UiStrings.Updating), K(UiStrings.SizeProgress)],
            UiStrings.UpdateProgress, UiStrings.Updating, 4_200_000, 52_400_000,
            OperationProgressType.MB));
        Add("progress-scan-acquiring", () => ShowProgress([K(UiStrings.AcquiringData)],
            "Canon CanoScan LiDE 400", UiStrings.AcquiringData, allowBackground: false), variant: true);
        Add("progress-scan-page", () => ShowProgress([K(UiStrings.ScanProgressPage)],
            "Canon CanoScan LiDE 400", string.Format(UiStrings.ScanProgressPage, 2), allowBackground: false));
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

        void Shortcuts(string name, bool bottom, bool variant = false)
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
                }), variant: variant);
        }
        Shortcuts("keyboard-shortcuts-top", false);
        Shortcuts("keyboard-shortcuts-bottom", true, variant: true);
    }

    private void DefineNotifications()
    {
        void Notify(string name, string[] strings, Action<INotify> action, bool variant = false) =>
            Add("notify-" + name, async () =>
            {
                LoadImages();
                Ctx.Ui(() => action(Get<INotify>()));
                await Ready(strings, DesktopTitle);
            }, variant: variant);

        Notify("pdf-saved", [K(UiStrings.PdfSaved)], n => n.PdfSaved(@"C:\Users\Example\Documents\scan.pdf"));
        Notify("image-saved", [K(UiStrings.ImageSaved)],
            n => n.ImagesSaved(1, @"C:\Users\Example\Pictures\scan.jpg"), variant: true);
        Notify("images-saved", [K(UiStrings.ImagesSaved)],
            n => n.ImagesSaved(4, @"C:\Users\Example\Pictures\scan_1.jpg"), variant: true);
        Notify("donate", [K(UiStrings.DonatePrompt)], n => n.DonatePrompt());
        Notify("review", [K(UiStrings.ReviewPrompt), K(UiStrings.LeaveAReview)], n => n.ReviewPrompt());
        Notify("update", [K(UiStrings.UpdateAvailable), K(UiStrings.Install)],
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
                [K(UiStrings.EnableDebugLogging), K(UiStrings.CheckingForUpdates)]),
            setup =>
            {
                setup.Config(c => c.CheckForUpdates, true);
                setup.FakeUpdateCheckNeverCompletes = true;
            });
        Add("about-no-updates", () => ShowForm<AboutForm>([K(UiStrings.NoUpdates)]),
            setup => setup.Config(c => c.CheckForUpdates, true), variant: true);
        Add("about-update", () => ShowForm<AboutForm>([K(UiStrings.Install)]),
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

        void ManualIp(string name, string[] strings, Action<ManualIpForm>? setState = null, bool variant = false) =>
            Add(name, () => ShowForm<ManualIpForm>(strings, before: f =>
            {
                TestScenarioUi.GetControls(f).OfType<TextBox>().First().Text = "192.168.1.25";
                setState?.Invoke(f);
            }), variant: variant);
        ManualIp("manual-ip-waiting", [K(UiStrings.WaitingToConnect)]);
        ManualIp("manual-ip-connecting", [K(UiStrings.Connecting)], f => f.SetConnecting(), variant: true);
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
                [K(UiStrings.DownloadProgressFormTitle), K(UiStrings.FilesProgressFormat), K(UiStrings.SizeProgress)],
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
            return ShowForm<EditProfileForm>([K(UiStrings.NamedPageSizeFormat), K(UiStrings.CustomPageSizeFormat)],
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

        void Batch(string name, string status, bool done, bool variant = true,
            [CallerArgumentExpression(nameof(status))] string key = "")
        {
            Add("batch-" + name, () =>
            {
                AddProfile();
                return ShowForm<BatchScanForm>(done ? [key, K(UiStrings.Close)] : [key],
                    f => f.SimulateStatus(status, done));
            }, variant: variant);
        }
        Batch("cancelled", UiStrings.BatchStatusCancelled, true);
        Batch("complete", UiStrings.BatchStatusComplete, true, variant: false);
        Batch("error", UiStrings.BatchStatusError, true);
        Batch("cancelling", UiStrings.BatchStatusCancelling, false);
        Batch("saving", UiStrings.BatchStatusSaving, false);
        Batch("scan-page", string.Format(UiStrings.BatchStatusScanPage, 3, 2), false, variant: false,
            key: K(UiStrings.BatchStatusScanPage));
        Batch("page", string.Format(UiStrings.BatchStatusPage, 3), false, key: K(UiStrings.BatchStatusPage));
        Batch("waiting", string.Format(UiStrings.BatchStatusWaitingForScan, 2), false,
            key: K(UiStrings.BatchStatusWaitingForScan));
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
        void ChooseDevice(string name, List<ScanDevice> devices, bool flatpakLink, string[] strings,
            bool variant = false)
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
                }, variant: variant);
        }
        ChooseDevice("list", ExampleDevices, true,
            [
                K(UiStrings.SaneDriver), K(UiStrings.AppleDriver), K(UiStrings.SelectSource),
                K(UiStrings.DevicesFound), K(UiStrings.CantFindScannerFlatpak)
            ]);
        ChooseDevice("single", [new ScanDevice(PlatformDriver, "unknown", SdkResources.UnknownScanner)], false,
            [K(UiStrings.DeviceFoundSingular), K(SdkResources.UnknownScanner)], variant: true);
        ChooseDevice("none", [], false, [K(UiStrings.NoDevicesFound)]);
    }

    private void DefineFileDialogs()
    {
        Add("file-dialog-save-images", () => ShowFileDialog(
            [
                K(UiStrings.FileTypeBmp), K(UiStrings.FileTypeJpeg), K(UiStrings.FileTypeJp2),
                K(UiStrings.FileTypeJxl), K(UiStrings.FileTypePng), K(UiStrings.FileTypeTiff),
                K(UiStrings.FileTypeWebp)
            ], FileFilterGroup.Image));
        Add("file-dialog-save-pdf", () => ShowFileDialog([K(UiStrings.FileTypePdf)],
            FileFilterGroup.Pdf | FileFilterGroup.Image), variant: true);
        Add("file-dialog-import", () => ShowFileDialog(
            [K(UiStrings.FileTypeAllFiles), K(UiStrings.FileTypeImageFiles), K(UiStrings.FileTypePdf)],
            FileFilterGroup.AllFiles | FileFilterGroup.Pdf | FileFilterGroup.AllImages | FileFilterGroup.Image,
            save: false));
    }

    private void OpenDropDown(string item) => Ctx.OpenDropDown(x => x == item);
}
#endif
