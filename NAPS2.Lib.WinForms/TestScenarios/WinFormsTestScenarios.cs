#if TEST_SCENARIOS
using Eto.Forms;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;
using NAPS2.WinForms;
using WF = System.Windows.Forms;
using SD = System.Drawing;
using Rectangle = Eto.Drawing.Rectangle;

namespace NAPS2.TestScenarios;

/// <summary>
/// Test scenarios that use WinForms directly (toolbars, tooltips, simulated macOS UI).
/// </summary>
internal class WinFormsTestScenarios : TestScenarioProvider
{
    protected override void Define()
    {
        DefineToolbar();
        DefineIconButtonToolTips();
        DefineMacSimulations();
        Add("preview", () =>
        {
            var images = LoadImages();
            return ShowForm<PreviewForm>(
                [
                    K(UiStrings.Previous), K(UiStrings.Next), K(UiStrings.ZoomIn), K(UiStrings.ZoomOut),
                    K(UiStrings.ZoomActual), K(UiStrings.ScaleWithWindow), K(UiStrings.XOfY),
                    K(UiStrings.PreviewFormTitle)
                ],
                f => f.CurrentImage = images[1],
                positions: f => ToolStripPositions(f.ToNative(), P(UiStrings.Previous), P(UiStrings.Next),
                    P(UiStrings.ZoomIn), P(UiStrings.ZoomOut), P(UiStrings.ZoomActual), P(UiStrings.ScaleWithWindow)));
        });
        Add("twain-waiting", async () =>
        {
            FTwainGui? gui = null;
            Ctx.Dispatch(() =>
            {
                gui = new FTwainGui();
                gui.ShowDialog(NativeDesktop);
            });
            // The form has no title, so WaitForNewWindow doesn't apply
            await Ctx.WaitUntil(() => Ctx.Ui(() => gui?.Visible ?? false), "the TWAIN form");
            await Ready([K(UiStrings.WaitingForTwain)], rect: () =>
            {
                var r = gui!.Bounds;
                r.Inflate(20, 20);
                return ToEto(r);
            });
        });
    }

    private WF.Form NativeDesktop => Desktop.ToNative();

    private WF.ToolStrip MainToolStrip => ((WinFormsDesktopForm) Desktop).MainToolStrip;

    private void DefineToolbar()
    {
        Add("menu-scan", async () =>
        {
            AddProfile();
            await Ready([K(UiStrings.NewProfile)], rect: Ctx.Ui(() => ShowToolbarMenu(UiStrings.Scan)));
        });
        Add("menu-image", async () =>
        {
            LoadImages();
            await Ready([K(UiStrings.EditWithAppName), K(UiStrings.EditWith)],
                rect: Ctx.Ui(() => ShowToolbarMenu(UiStrings.Image)));
        }, setup =>
        {
            setup.Config(c => c.EditWithAppName, "Paint");
            setup.Config(c => c.EditWithAppPath, @"C:\Windows\System32\mspaint.exe");
        });
        Add("menu-reorder", async () =>
        {
            LoadImages(4, 2);
            await Ready([K(UiStrings.ManualDuplexPreview)], rect: Ctx.Ui(() => ShowToolbarMenu(UiStrings.Reorder)));
        }, setup => setup.Config(c => c.ManualDuplexSettings.AlwaysShowPreview, false));
        Add("menu-reorder-reverse", async () =>
        {
            LoadImages(4, 2);
            await Ready([], rect: Ctx.Ui(() => ShowToolbarMenu(UiStrings.Reorder, UiStrings.Reverse)));
        });
        Add("toolbar-context-menu", async () =>
        {
            LoadImages();
            await Ready([K(UiStrings.CustomizeToolbar)], DesktopTitle, popupCrop: 120, before: () =>
                MainToolStrip.ContextMenuStrip!.Show(MainToolStrip, new SD.Point(400, 30)));
        });
    }

    /// <summary>
    /// Opens a toolbar dropdown menu (and submenus) and returns a capture rect covering the button and menus.
    /// </summary>
    private Func<Rectangle> ShowToolbarMenu(string text, params string[] subMenuPath)
    {
        var item = MainToolStrip.Items.OfType<WF.ToolStripDropDownItem>().FirstOrDefault(i => TextEquals(i.Text, text))
                   ?? throw new TestScenarioException($"No toolbar item '{text}'");
        item.ShowDropDown();
        var menus = new List<WF.ToolStripDropDown> { item.DropDown };
        var current = item;
        foreach (var sub in subMenuPath)
        {
            current = current.DropDownItems.OfType<WF.ToolStripDropDownItem>().First(i => TextEquals(i.Text, sub));
            current.Select();
            current.ShowDropDown();
            menus.Add(current.DropDown);
        }
        return () =>
        {
            var r = menus.Select(m => m.Bounds).Append(item.Owner!.RectangleToScreen(item.Bounds))
                .Aggregate(SD.Rectangle.Union);
            r.Inflate(12, 12);
            return ToEto(r);
        };
    }

    /// <summary>
    /// Gets tag positions for (icon-only) toolstrip items matched by text or tooltip.
    /// </summary>
    private static Dictionary<string, Rectangle> ToolStripPositions(WF.Control root,
        params (string Key, string Text)[] items)
    {
        var result = new Dictionary<string, Rectangle>();
        var toolStripItems = AllControls(root).OfType<WF.ToolStrip>()
            .SelectMany(ts => ts.Items.Cast<WF.ToolStripItem>()).ToList();
        foreach (var (key, text) in items)
        {
            var item = toolStripItems.FirstOrDefault(i =>
                TextEquals(i.Text, text) || TextEquals(i.ToolTipText, text));
            if (item?.Owner != null && item.Visible)
            {
                result[key] = ToEto(item.Owner.RectangleToScreen(item.Bounds));
            }
        }
        return result;
    }

    private void DefineIconButtonToolTips()
    {
        void Combine(string name, CombineOrientation orientation, (string Key, string Text)[] buttons,
            bool variant = false)
        {
            Add(name, () =>
            {
                var images = LoadImages(2);
                return ShowForm<CombineForm>(buttons.Select(b => b.Key).ToArray(),
                    f => SetImage(f, images[0]),
                    before: f =>
                    {
                        f.SetOrientation(orientation);
                        WF.Application.DoEvents();
                        ShowButtonToolTips(f.ToNative(), buttons.Select(b => b.Text).ToArray());
                    });
            }, variant: variant);
        }
        Combine("combine-tooltips-vertical", CombineOrientation.Vertical,
            [P(UiStrings.AlignLeft), P(UiStrings.AlignCenter), P(UiStrings.AlignRight), P(UiStrings.ToggleOrientation),
                P(UiStrings.Swap)]);
        Combine("combine-tooltips-horizontal", CombineOrientation.Horizontal,
            [P(UiStrings.AlignTop), P(UiStrings.AlignMiddle), P(UiStrings.AlignBottom), P(UiStrings.ToggleOrientation),
                P(UiStrings.Swap)], variant: true);
        Add("split-tooltips", () =>
        {
            var images = LoadImages();
            return ShowForm<SplitForm>(
                [K(UiStrings.SplitVertically), K(UiStrings.SplitHorizontally), K(UiStrings.Help)],
                f => SetImage(f, images[0]),
                before: f => ShowButtonToolTips(f.ToNative(), UiStrings.SplitVertically, UiStrings.SplitHorizontally,
                    UiStrings.Help));
        });
        Add("choose-device-tooltips", () => ShowForm<ChooseDeviceForm>([K(UiStrings.ToggleView)],
                f => f.ScanOptions = new ScanOptions { Driver = PlatformDriver },
                before: f => ShowButtonToolTips(f.ToNative(), UiStrings.ToggleView)),
            setup => setup.FakeDevices = ExampleDevices);
    }

    /// <summary>
    /// Shows the tooltips of the given icon buttons (found by their accessible names) all at once. Each one is next to
    /// its button (above it, or below if the button is near the top of the form), moving further away only as needed
    /// to avoid overlapping another tooltip.
    /// </summary>
    private static void ShowButtonToolTips(WF.Form form, params string[] names)
    {
        const int rowHeight = 26;
        var buttons = names
            .Select(name => AllControls(form).OfType<WF.Button>().First(b => b.Visible && b.AccessibleName == name))
            .OrderBy(b => b.PointToScreen(SD.Point.Empty).X)
            .ToList();
        // The right edge of the last tooltip placed in each row
        var rowEnds = new List<int>();
        foreach (var button in buttons)
        {
            int left = button.PointToScreen(SD.Point.Empty).X;
            int width = WF.TextRenderer.MeasureText(button.AccessibleName, SD.SystemFonts.StatusFont).Width + 16;
            int row = rowEnds.FindIndex(end => end < left);
            if (row == -1)
            {
                row = rowEnds.Count;
                rowEnds.Add(0);
            }
            rowEnds[row] = left + width;
            bool below = form.PointToClient(button.PointToScreen(SD.Point.Empty)).Y < form.ClientSize.Height / 2;
            int y = below ? button.Height + 4 + row * rowHeight : -(row + 1) * rowHeight;
            new WF.ToolTip().Show(button.AccessibleName, button, new SD.Point(0, y), 60000);
        }
    }

    private void DefineMacSimulations()
    {
        Add("mac-menu-file", async () =>
        {
            LoadImages();
            await Ready([K(UiStrings.EmailAll), K(UiStrings.EmailSelected), K(UiStrings.ClearAll)],
                rect: Ctx.Ui(() => ShowSimulatedMacMenu("File")));
        });
        Add("mac-menu-tools", async () =>
        {
            LoadImages();
            await Ready([K(UiStrings.Tools)], rect: Ctx.Ui(() => ShowSimulatedMacMenu(UiStrings.Tools)));
        });
        Add("mac-zoom-tooltip", async () =>
        {
            LoadImages();
            // The macOS toolbar has a thumbnail zoom slider with a "Zoom" tooltip
            var rect = Ctx.Ui(() =>
            {
                var slider = new WF.TrackBar
                    { Minimum = 0, Maximum = 10, Value = 5, TickStyle = WF.TickStyle.None, Width = 100 };
                NativeDesktop.Controls.Add(slider);
                slider.Location = new SD.Point(NativeDesktop.ClientSize.Width - 130, 100);
                slider.BringToFront();
                new WF.ToolTip().Show(UiStrings.Zoom, slider, new SD.Point(20, slider.Height + 4), 60000);
                return (Func<Rectangle>) (() =>
                {
                    var r = slider.RectangleToScreen(slider.ClientRectangle);
                    r.Inflate(80, 60);
                    r.Offset(-40, 20);
                    return ToEto(r);
                });
            });
            await Ready([K(UiStrings.Zoom)], rect: rect);
        });
    }

    /// <summary>
    /// Simulates the macOS menu bar (see MacDesktopForm.CreateToolbarsAndMenus) with a WinForms MenuStrip.
    /// </summary>
    private Func<Rectangle> ShowSimulatedMacMenu(string open)
    {
        WF.ToolStripMenuItem Menu(string text, params string?[] items)
        {
            var menu = new WF.ToolStripMenuItem(text);
            foreach (var item in items)
            {
                menu.DropDownItems.Add(item == null ? new WF.ToolStripSeparator() : new WF.ToolStripMenuItem(item));
            }
            return menu;
        }
        var strip = new WF.MenuStrip();
        strip.Items.AddRange(new WF.ToolStripItem[]
        {
            Menu("NAPS2", UiStrings.About, null, UiStrings.Settings, UiStrings.Language),
            Menu("File", UiStrings.Import, null, UiStrings.SaveAll, UiStrings.SaveSelected, null,
                UiStrings.EmailAll, UiStrings.EmailSelected, UiStrings.Print, null, UiStrings.PdfSettings,
                UiStrings.ImageSettings, UiStrings.EmailSettings, null, UiStrings.ClearAll),
            Menu(UiStrings.Edit, UiStrings.SelectAll, UiStrings.Copy, UiStrings.Paste, null, UiStrings.Undo,
                UiStrings.Redo, null, UiStrings.Delete),
            Menu(UiStrings.Scan, "My Scanner", null, UiStrings.NewProfile, UiStrings.BatchScan),
            Menu(UiStrings.Image),
            Menu(UiStrings.Reorder),
            Menu(UiStrings.Tools, UiStrings.Profiles, UiStrings.BatchScan, UiStrings.ScannerSharing, UiStrings.Ocr)
        });
        NativeDesktop.Controls.Add(strip);
        strip.Dock = WF.DockStyle.Top;
        strip.BringToFront();
        var menuItem = strip.Items.Cast<WF.ToolStripMenuItem>().First(i => i.Text == open);
        menuItem.ShowDropDown();
        return () =>
        {
            var r = SD.Rectangle.Union(strip.RectangleToScreen(strip.ClientRectangle), menuItem.DropDown.Bounds);
            r.Inflate(0, 8);
            r.Width = Math.Min(r.Width, menuItem.DropDown.Bounds.Right - r.Left + 120);
            return ToEto(r);
        };
    }

    private static IEnumerable<WF.Control> AllControls(WF.Control root) =>
        root.Controls.Cast<WF.Control>().SelectMany(c => new[] { c }.Concat(AllControls(c)));

    private static bool TextEquals(string? a, string b) => TestScenarioUi.Normalize(a) == TestScenarioUi.Normalize(b);

    private static Rectangle ToEto(SD.Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
}
#endif
