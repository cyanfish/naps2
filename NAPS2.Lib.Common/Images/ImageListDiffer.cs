using System.Collections.Immutable;

namespace NAPS2.Images;

public class ImageListDiffer
{
    private readonly UiImageList _imageList;
    private List<ImageRenderState> _currentState = new();

    public ImageListDiffer(UiImageList imageList)
    {
        _imageList = imageList;
    }

    public ImageListDiffs GetAndFlushDiffs()
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
            var appendOps = ImmutableList<ImageListDiffs.AppendOperation>.Empty;
            foreach (var image in newState.Skip(_currentState.Count))
            {
                appendOps = appendOps.Add(new ImageListDiffs.AppendOperation(image));
            }

            var replaceOps = ImmutableList<ImageListDiffs.ReplaceOperation>.Empty;
            for (int i = 0; i < Math.Min(_currentState.Count, newState.Count); i++)
            {
                if (!Equals(_currentState[i], newState[i]))
                {
                    replaceOps = replaceOps.Add(new ImageListDiffs.ReplaceOperation(i, newState[i]));
                }
            }

            var trimOps = ImmutableList<ImageListDiffs.TrimOperation>.Empty;
            if (newState.Count < _currentState.Count)
            {
                trimOps = trimOps.Add(new ImageListDiffs.TrimOperation(_currentState.Count - newState.Count));
            }

            _currentState = newState;
            return new ImageListDiffs
            {
                AppendOperations = appendOps,
                ReplaceOperations = replaceOps,
                TrimOperations = trimOps
            };
        }
    }
}