using System.Collections.Immutable;

namespace NAPS2.Images;

using AppendOperation = ListViewDiffs<UiImage>.AppendOperation;
using ReplaceOperation = ListViewDiffs<UiImage>.ReplaceOperation;
using TrimOperation = ListViewDiffs<UiImage>.TrimOperation;

public class ImageListDiffer
{
    private readonly UiImageList _imageList;
    private List<ImageRenderState> _currentState = new();

    public ImageListDiffer(UiImageList imageList)
    {
        _imageList = imageList;
    }

    public ListViewDiffs<UiImage> GetAndFlushDiffs()
    {
        lock (this)
        {
            List<ImageRenderState> newState;
            lock (_imageList)
            {
                newState = _imageList.Images.Select(x => x.GetImageRenderState()).ToList();
            }

            // TODO: We do actually need the UiImage reference somehow
            // TODO: Or actually UiThumbnailProvider goes against this whole thing being fully immutable... 
            var appendOps = ImmutableList<AppendOperation>.Empty;
            foreach (var image in newState.Skip(_currentState.Count))
            {
                appendOps = appendOps.Add(new AppendOperation(image.Source));
            }

            // TODO: "Replace" might also mean "refresh", i.e. we have the same image object but it's been updated
            // TODO: Is it worth representing that somehow?
            var replaceOps = ImmutableList<ReplaceOperation>.Empty;
            for (int i = 0; i < Math.Min(_currentState.Count, newState.Count); i++)
            {
                if (!Equals(_currentState[i], newState[i]))
                {
                    replaceOps = replaceOps.Add(new ReplaceOperation(i, newState[i].Source));
                }
            }

            var trimOps = ImmutableList<TrimOperation>.Empty;
            if (newState.Count < _currentState.Count)
            {
                trimOps = trimOps.Add(new TrimOperation(_currentState.Count - newState.Count));
            }

            _currentState = newState;
            return new ListViewDiffs<UiImage>
            {
                AppendOperations = appendOps,
                ReplaceOperations = replaceOps,
                TrimOperations = trimOps
            };
        }
    }
}