using Eto.Drawing;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Desktop;

public class EditWithController(
    ScanningContext scanningContext,
    UiImageList imageList,
    Naps2Config config,
    IOpenWith openWith,
    ErrorOutput errorOutput,
    IFormFactory formFactory)
{
    public event EventHandler? AppChanged;

    public bool HasApp => !string.IsNullOrEmpty(config.Get(c => c.EditWithAppName));

    public Image? AppIcon { get; private set; }

    public async Task LoadAppIcon()
    {
        // Load the icon of the configured app in the background
        string? appId = config.Get(c => c.EditWithAppPath);
        if (string.IsNullOrEmpty(appId))
        {
            return;
        }
        using var icon = await Task.Run(() =>
        {
            try
            {
                var entry = openWith.GetEntries(".jpg").FirstOrDefault(entry => entry.Id == appId);
                return entry != null ? openWith.LoadIcon(entry) : null;
            }
            catch (Exception ex)
            {
                Log.ErrorException($"Error loading icon for {appId}", ex);
                return null;
            }
        });
        SetAppIcon(icon);
    }

    private void SetAppIcon(IMemoryImage? icon)
    {
        AppIcon = icon?.ToEtoImage();
        AppChanged?.Invoke(this, EventArgs.Empty);
    }

    public void EditWithApp(IEnumerable<UiImage> selection)
    {
        string? appId = config.Get(c => c.EditWithAppPath);
        if (string.IsNullOrEmpty(appId))
        {
            EditWithPick(selection);
            return;
        }
        EditWithPath(appId, selection);
    }

    public void EditWithPick(IEnumerable<UiImage> selection)
    {
        var pickForm = formFactory.Create<EditWithForm>();
        pickForm.ShowModal();
        var entry = pickForm.Result;
        if (entry != null)
        {
            var transact = config.User.BeginTransaction();
            transact.Set(c => c.EditWithAppPath, entry.Id);
            transact.Set(c => c.EditWithAppName, entry.Name);
            transact.Commit();
            using var icon = openWith.LoadIcon(entry);
            SetAppIcon(icon);
            EditWithPath(entry.Id, selection);
        }
    }

    private void EditWithPath(string appPath, IEnumerable<UiImage> selection)
    {
        var tempFilePaths = new List<string>();
        foreach (var uiImage in selection)
        {
            if (!uiImage.IsDisposed)
            {
                var editorSession = new ExternalEditorSession(scanningContext, imageList, uiImage);
                uiImage.EditorSessions.Add(editorSession);
                tempFilePaths.Add(editorSession.TempFilePath);
            }
        }
        try
        {
            openWith.OpenWith(appPath, tempFilePaths);
        }
        catch (Exception ex)
        {
            errorOutput.DisplayError(string.Format(UiStrings.ErrorStartingApplication, appPath), ex);
        }
    }
}