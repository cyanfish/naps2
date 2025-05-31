using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NAPS2.Images.Gdi;

namespace NAPS2.Platform.Windows;

public class WindowsOpenWith : IOpenWith
{
    private static readonly Guid BHID_DataObject = new("b8c0bd9f-ed24-455c-83e6-d5390c4fe8c4");

    private readonly ImageContext _imageContext;

    public WindowsOpenWith(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public IEnumerable<OpenWithEntry> GetEntries(string fileExt)
    {
        foreach (var handler in EnumerateHandlers(fileExt))
        {
            handler.GetUIName(out var uiName);
            handler.GetName(out var internalName);
            handler.GetIconLocation(out var iconPath, out var iconIndex);
            yield return new OpenWithEntry(internalName, uiName, iconPath, iconIndex);
        }
    }

    public void OpenWith(string entryId, IEnumerable<string> filePaths)
    {
        var pidls = new List<IntPtr>();
        foreach (var path in filePaths)
        {
            int hr = Win32.SHParseDisplayName(path, IntPtr.Zero, out var pidl, 0, out _);
            if (hr != 0) throw new Win32Exception(hr);
            pidls.Add(pidl);
        }
        try
        {
            int hr = Win32.SHCreateShellItemArrayFromIDLists(pidls.Count, pidls.ToArray(), out var shellItemArray);
            if (hr != 0) throw new Win32Exception(hr);

            foreach (var handler in EnumerateHandlers(Path.GetExtension(filePaths.First())))
            {
                handler.GetName(out var internalName);
                if (internalName == entryId)
                {
                    var dao = shellItemArray.BindToHandler(null, BHID_DataObject, typeof(IDataObject).GUID);
                    handler.Invoke(dao);
                }
            }
        }
        finally
        {
            foreach (var pidl in pidls)
            {
                Win32.ILFree(pidl);
            }
        }
    }

    public IMemoryImage? LoadIcon(OpenWithEntry entry)
    {
        if (entry.IconPath.StartsWith("@"))
        {
            StringBuilder outBuff = new StringBuilder(1024);
            int hr = Win32.SHLoadIndirectString(entry.IconPath, outBuff, outBuff.Capacity, IntPtr.Zero);
            if (hr != 0) throw new Win32Exception(hr);
            return _imageContext.Load(outBuff.ToString());
        }
        var icon = Icon.ExtractIcon(entry.IconPath, entry.IconIndex);
        if (icon == null) return null;
        return new GdiImage(icon.ToBitmap());
    }

    private IEnumerable<Win32.IAssocHandler> EnumerateHandlers(string fileExt)
    {
        int hr = Win32.SHAssocEnumHandlers(fileExt, Win32.ASSOC_FILTER.ASSOC_FILTER_RECOMMENDED,
            out Win32.IEnumAssocHandlers eah);
        if (hr != 0) throw new Win32Exception(hr);
        while (eah.Next(1, out var handler, out var fetched) == 0 && fetched > 0)
        {
            yield return handler;
        }
    }
}