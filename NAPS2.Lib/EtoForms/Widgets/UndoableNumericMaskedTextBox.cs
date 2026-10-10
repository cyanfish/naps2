using Eto.Forms;

namespace NAPS2.EtoForms.Widgets;

/// <summary>
/// A NumericMaskedTextBox that supports undo/redo.
///
/// Eto's MaskedTextBox applies every edit by cancelling the native edit and setting the text itself, which leaves the
/// native undo history empty. Instead we keep our own history of text values.
/// </summary>
public class UndoableNumericMaskedTextBox<T> : NumericMaskedTextBox<T>
{
    private const int MAX_HISTORY = 20;

    private readonly List<string> _history = [];
    private int _index = -1;
    private bool _restoring;

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (_restoring || (_index >= 0 && Text == _history[_index])) return;
        // A new edit discards any history that could have been redone
        _history.RemoveRange(_index + 1, _history.Count - _index - 1);
        _history.Add(Text);
        if (_history.Count > MAX_HISTORY)
        {
            _history.RemoveAt(0);
        }
        _index = _history.Count - 1;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var mod = Application.Instance.CommonModifier;
        // Match the platform conventions (and the app's undo/redo shortcuts): Ctrl+Y on Windows, Mod+Shift+Z elsewhere
        var redoKeys = EtoPlatform.Current.IsWinForms ? mod | Keys.Y : mod | Keys.Shift | Keys.Z;
        if (e.KeyData == (mod | Keys.Z))
        {
            Restore(_index - 1);
            e.Handled = true;
            return;
        }
        if (e.KeyData == redoKeys)
        {
            Restore(_index + 1);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void Restore(int index)
    {
        if (index < 0 || index >= _history.Count) return;
        _index = index;
        _restoring = true;
        try
        {
            Text = _history[index];
            CaretIndex = Text.Length;
        }
        finally
        {
            _restoring = false;
        }
    }
}
