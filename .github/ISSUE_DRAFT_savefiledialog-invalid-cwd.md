# Bug: SaveFileDialog crashes when current working directory is deleted/unmounted at runtime

## Summary
On Linux (GTK/Eto), NAPS2 can throw an unhandled `System.IO.FileNotFoundException` when opening **Save Images** if the process current working directory becomes invalid after startup (for example, removable media unmounted or directory removed).

The dialog never opens and users may be unable to save scanned pages.

## Affected area
- `NAPS2.Lib/EtoForms/EtoDialogHelper.cs`
- GTK `SaveFileDialog` / `OpenFileDialog` construction path

## Observed stack trace
```text
System.IO.FileNotFoundException: Unable to find the specified file.
   at string Interop+Sys.GetCwd()
   at string Environment.get_CurrentDirectoryCore()
   at string Environment.get_CurrentDirectory()
   at string System.IO.Directory.GetCurrentDirectory()
   at new Eto.GtkSharp.Forms.SaveFileDialogHandler()
   at bool NAPS2.EtoForms.EtoDialogHelper.PromptToSaveImage(...)
   at async Task<bool> NAPS2.ImportExport.ExportController.SaveImages(...)
```

## Steps to reproduce
1. Launch NAPS2 from a directory that is later removed or unmounted.
2. Scan/import pages.
3. Click **Save Images**.

## Expected behavior
NAPS2 should recover from an invalid CWD and still open the file dialog, falling back to a safe directory (typically user home).

## Actual behavior
An unhandled exception is thrown while creating the dialog handler and the save dialog does not open.

## Proposed resolution
Harden dialog creation in `EtoDialogHelper`:

- Before creating dialogs, verify current directory is accessible.
- If inaccessible (exception or non-existent path), set `Environment.CurrentDirectory` to:
  1. `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)`
  2. fallback `Path.GetTempPath()` if needed
- Wrap `SaveFileDialog` construction in a retry path for `FileNotFoundException`.
- Apply the same CWD guard before `OpenFileDialog` creation.

## Patch status
Implemented on branch:
- `fix/savefiledialog-invalid-cwd-crash`

Files changed:
- `NAPS2.Lib/EtoForms/EtoDialogHelper.cs`

## Validation done
- File-level diagnostics clean for `EtoDialogHelper.cs`.
- Full build was not run in this environment (`dotnet` CLI unavailable here).

## Suggested reviewer checklist
- [ ] Reproduce with invalid CWD on Linux/GTK before patch
- [ ] Confirm **Save Images** opens after patch
- [ ] Confirm **Import** dialog also opens with invalid CWD
- [ ] Confirm no regressions in normal save/import flows
