using System.Collections.Immutable;

namespace NAPS2.Images;

using AppendOperation = ListViewDiffs<UiImage>.AppendOperation;
using ReplaceOperation = ListViewDiffs<UiImage>.ReplaceOperation;
using TrimOperation = ListViewDiffs<UiImage>.TrimOperation;

/// <summary>
/// Atomically produces changes made to a UiImageList.
/// </summary>
public class ImageListDiffer
{
    private readonly UiImageList _imageList;
    private List<ImageRenderState> _currentState = [];

    public ImageListDiffer(UiImageList imageList)
    {
        _imageList = imageList;
    }

    /// <summary>
    /// Produces the set of changes since the last call to GetAndFlushDiffs. The first call assumes the previous state
    /// is an empty list, i.e. all items in the list are included as appended items in the diff.
    ///
    /// In addition to the images themselves being added/deleted/replaced, if the image's render state changes (i.e. its
    /// transforms or thumbnail change) it will also be included in the diff. 
    /// </summary>
    /// <returns>The changes/diffs.</returns>
    public ListViewDiffs<UiImage> GetAndFlushDiffs()
    {
        lock (this)
        {
            List<ImageRenderState> newState;
            lock (_imageList)
            {
                newState = _imageList.Images.Select(x => x.GetImageRenderState()).ToList();
            }
 
            var appendOps = ImmutableList<AppendOperation>.Empty;
            foreach (var image in newState.Skip(_currentState.Count))
            {
                appendOps = appendOps.Add(new AppendOperation(image.Source));
            }

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
            return new ListViewDiffs<UiImage>(appendOps, replaceOps, trimOps);
        }
    }
}