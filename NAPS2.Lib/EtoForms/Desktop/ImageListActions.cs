using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport;

namespace NAPS2.EtoForms.Desktop;

public class ImageListActions
{
    private readonly UiImageList _imageList;
    private readonly IOperationFactory _operationFactory;
    private readonly OperationProgress _operationProgress;
    private readonly Naps2Config _config;
    private readonly ThumbnailController _thumbnailController;
    private readonly IExportController _exportController;
    private readonly INotify _notify;

    public ImageListActions(UiImageList imageList, IOperationFactory operationFactory,
        OperationProgress operationProgress, Naps2Config config, ThumbnailController thumbnailController,
        IExportController exportController, INotify notify)
    {
        _imageList = imageList;
        _operationFactory = operationFactory;
        _operationProgress = operationProgress;
        _config = config;
        _thumbnailController = thumbnailController;
        _exportController = exportController;
        _notify = notify;
    }

    private Func<ListSelection<UiImage>>? SelectionFunc { get; init; }

    private ListSelection<UiImage>? Selection => SelectionFunc?.Invoke();

    public ImageListActions WithSelection(Func<ListSelection<UiImage>> selectionFunc)
    {
        return new ImageListActions(_imageList, _operationFactory, _operationProgress, _config, _thumbnailController,
            _exportController, _notify)
        {
            SelectionFunc = selectionFunc
        };
    }

    public void MoveDown() => _imageList.Mutate(new ImageListMutation.MoveDown(), Selection);

    public void MoveUp() => _imageList.Mutate(new ImageListMutation.MoveUp(), Selection);

    public void MoveTo(int index) => _imageList.Mutate(new ImageListMutation.MoveTo(index), Selection);

    public void DeleteAll() => _imageList.Mutate(new ImageListMutation.DeleteAll(), Selection);

    public void DeleteSelected() => _imageList.Mutate(new ImageListMutation.DeleteSelected(), Selection);

    public void Interleave() => _imageList.Mutate(new ImageListMutation.Interleave(), Selection);

    public void Deinterleave() => _imageList.Mutate(new ImageListMutation.Deinterleave(), Selection);

    public void AltInterleave() => _imageList.Mutate(new ImageListMutation.AltInterleave(), Selection);

    public void AltDeinterleave() => _imageList.Mutate(new ImageListMutation.AltDeinterleave(), Selection);

    public void ReverseAll() => _imageList.Mutate(new ImageListMutation.ReverseAll(), Selection);

    public void ReverseSelected() => _imageList.Mutate(new ImageListMutation.ReverseSelection(), Selection);

    public async Task RotateLeft() =>
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(270), Selection);

    public async Task RotateRight() =>
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(90), Selection);

    public async Task Flip() => await _imageList.MutateAsync(new ImageListMutation.RotateFlip(180), Selection);

    public async Task AltFlip() =>
        await _imageList.MutateAsync(new ImageListMutation.AltFlip(), Selection);

    public void DocumentCorrection() =>
        _imageList.Mutate(new ImageListMutation.AddTransforms([new CorrectionTransform(CorrectionMode.Document)]),
            Selection);

    // TODO: Does it make sense to move this method somewhere else?
    public void Deskew()
    {
        var images = Selection ?? _imageList.Selection;
        if (!images.Any())
        {
            return;
        }

        var op = _operationFactory.Create<DeskewOperation>();
        if (op.Start(_imageList, images.ToList(), new DeskewParams { ThumbnailSize = _thumbnailController.RenderSize }))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    public async Task RotateFlip(double angle) =>
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(angle), Selection);

    public void ResetTransforms() => _imageList.Mutate(new ImageListMutation.ResetTransforms(), Selection);

    public void SelectAll() => _imageList.UpdateSelection(ListSelection.From(_imageList.Images));

    public async Task Undo() => await _imageList.Undo();

    public async Task Redo() => await _imageList.Redo();

    public Task SaveAllAsPdf() => _exportController.SavePdf(_imageList.Images, _notify);
    public Task SaveSelectedAsPdf() => _exportController.SavePdf(_imageList.Selection, _notify);
    public Task SaveAllAsImages() => _exportController.SaveImages(_imageList.Images, _notify);
    public Task SaveSelectedAsImages() => _exportController.SaveImages(_imageList.Selection, _notify);
    public Task SaveAllAsPdfOrImages() => _exportController.SavePdfOrImages(_imageList.Images, _notify);
    public Task SaveSelectedAsPdfOrImages() => _exportController.SavePdfOrImages(_imageList.Selection, _notify);
    public Task EmailAllAsPdf() => _exportController.EmailPdf(_imageList.Images);
    public Task EmailSelectedAsPdf() => _exportController.EmailPdf(_imageList.Selection);
}