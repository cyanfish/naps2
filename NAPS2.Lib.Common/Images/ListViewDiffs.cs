using System.Collections.Immutable;

namespace NAPS2.Images;

public record ListViewDiffs<T>(
    ImmutableList<ListViewDiffs<T>.AppendOperation> AppendOperations,
    ImmutableList<ListViewDiffs<T>.ReplaceOperation> ReplaceOperations,
    ImmutableList<ListViewDiffs<T>.TrimOperation> TrimOperations)
{
    // TODO: It might make sense to rework the diff model to allow images to be reused, it depends on how the list perf works
    // TODO: The old model was:
    // 1. Delete images not present in the old one
    // 2. Append images beyond the count of the updated list
    // 3. Replace images within the current list that have updated transforms
    // This model doesn't really make sense anymore though as there's no differentiation between delete+add and update.
    // Another way to look at it is minimizing the number of changes to the list which is an algorithm problem. 

    public bool HasAnyDiff => AppendOperations.Any() || ReplaceOperations.Any() || TrimOperations.Any();

    public record AppendOperation(T Item);

    public record ReplaceOperation(int Index, T Item);

    public record TrimOperation(int Count);
}