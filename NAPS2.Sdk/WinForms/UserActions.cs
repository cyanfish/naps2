using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.WinForms
{
    public class UserActions
    {
        private readonly ImageContext imageContext;
        private readonly ScannedImageList imageList;

        public UserActions(ImageContext imageContext, ScannedImageList imageList)
        {
            this.imageList = imageList;
            this.imageContext = imageContext;
        }
        
        public void MoveDown() => imageList.Mutate(new ImageListMutation.MoveDown());
        
        public void MoveUp() => imageList.Mutate(new ImageListMutation.MoveUp());

        public void MoveTo(int index) => imageList.Mutate(new ImageListMutation.MoveTo(index));

        public void DeleteAll() => imageList.Mutate(new ImageListMutation.DeleteAll());
        
        public void DeleteSelected() => imageList.Mutate(new ImageListMutation.DeleteSelected());

        public void Interleave() => imageList.Mutate(new ImageListMutation.Interleave());

        public void Deinterleave() => imageList.Mutate(new ImageListMutation.Deinterleave());

        public void AltInterleave() => imageList.Mutate(new ImageListMutation.AltInterleave());

        public void AltDeinterleave() => imageList.Mutate(new ImageListMutation.AltDeinterleave());

        public void ReverseAll() => imageList.Mutate(new ImageListMutation.ReverseAll());

        public void ReverseSelected() => imageList.Mutate(new ImageListMutation.ReverseSelection());

        public async Task RotateLeft() => await imageList.MutateAsync(new ImageListMutation.RotateFlip(imageContext, 270));

        public async Task RotateRight() => await imageList.MutateAsync(new ImageListMutation.RotateFlip(imageContext, 90));

        public async Task Flip() => await imageList.MutateAsync(new ImageListMutation.RotateFlip(imageContext, 180));

        public void Deskew()
        {
            // TODO
//            if (!SelectedIndices.Any())
//            {
//                return;
//            }
//
//            var op = operationFactory.Create<DeskewOperation>();
//            if (op.Start(SelectedImages.ToList(), new DeskewParams { ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize) }))
//            {
//                operationProgress.ShowProgress(op);
//                changeTracker.Made();
//            }
        }

        public async Task RotateFlip(double angle) => await imageList.MutateAsync(new ImageListMutation.RotateFlip(imageContext, angle));

        public void ResetTransforms() => imageList.Mutate(new ImageListMutation.ResetTransforms());

        public void SelectAll()
        {
            imageList.UpdateSelection(ListSelection.From(imageList.Images));
        }
    }
}
