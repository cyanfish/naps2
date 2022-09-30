using System.Collections.Immutable;

namespace NAPS2.Images;

/// <summary>
/// A representation of changes to a list of items.
/// </summary>
/// <param name="AppendOperations">
/// If the new list is longer, any additional items (even if they were previous present earlier in the list) are
/// represented as append operations.
/// </param>
/// <param name="ReplaceOperations">
/// Any change to the item at an existing index in the list is represented as a replace operation. An item can be
/// replaced by itself if it is updated in a way that that causes it to need re-rendering. There is no concept of
/// "moving" items, any changes to multiple indexes are represented by multiple replaces.
/// </param>
/// <param name="TrimOperations">
/// If the new list is shorter, the difference is represented by a single trim operation.
/// </param>
/// <typeparam name="T"></typeparam>
// TODO: This model works well enough, but we could consider adding some kind of "move" operation if that ends up being more performant for the listview.
public record ListViewDiffs<T>(
    ImmutableList<ListViewDiffs<T>.AppendOperation> AppendOperations,
    ImmutableList<ListViewDiffs<T>.ReplaceOperation> ReplaceOperations,
    ImmutableList<ListViewDiffs<T>.TrimOperation> TrimOperations)
{
    public bool HasAnyDiff => AppendOperations.Any() || ReplaceOperations.Any() || TrimOperations.Any();

    public record AppendOperation(T Item);

    public record ReplaceOperation(int Index, T Item);

    public record TrimOperation(int Count);
}