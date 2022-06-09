namespace NAPS2.WinForms;

public class UserActions
{
    private readonly ImageContext _imageContext;
    private readonly UiImageList _imageList;

    public UserActions(ImageContext imageContext, UiImageList imageList)
    {
        _imageList = imageList;
        _imageContext = imageContext;
    }
        
    public void MoveDown() => _imageList.Mutate(new ImageListMutation.MoveDown());
        
    public void MoveUp() => _imageList.Mutate(new ImageListMutation.MoveUp());

    public void MoveTo(int index) => _imageList.Mutate(new ImageListMutation.MoveTo(index));

    public void DeleteAll() => _imageList.Mutate(new ImageListMutation.DeleteAll());
        
    public void DeleteSelected() => _imageList.Mutate(new ImageListMutation.DeleteSelected());

    public void Interleave() => _imageList.Mutate(new ImageListMutation.Interleave());

    public void Deinterleave() => _imageList.Mutate(new ImageListMutation.Deinterleave());

    public void AltInterleave() => _imageList.Mutate(new ImageListMutation.AltInterleave());

    public void AltDeinterleave() => _imageList.Mutate(new ImageListMutation.AltDeinterleave());

    public void ReverseAll() => _imageList.Mutate(new ImageListMutation.ReverseAll());

    public void ReverseSelected() => _imageList.Mutate(new ImageListMutation.ReverseSelection());

    public async Task RotateLeft() => await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 270));

    public async Task RotateRight() => await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 90));

    public async Task Flip() => await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 180));

    public void Deskew()
    {
        // TODO
//            if (!SelectedIndices.Any())
//            {
//                return;
//            }
//
//            var op = operationFactory.Create<DeskewOperation>();
//            if (op.Start(SelectedImages.ToList(), new DeskewParams { ThumbnailSize = Config.Get(c => c.ThumbnailSize) }))
//            {
//                operationProgress.ShowProgress(op);
//                changeTracker.Made();
//            }
    }

    public async Task RotateFlip(double angle) => await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, angle));

    public void ResetTransforms() => _imageList.Mutate(new ImageListMutation.ResetTransforms());

    public void SelectAll()
    {
        _imageList.UpdateSelection(ListSelection.From(_imageList.Images));
    }
}