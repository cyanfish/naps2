namespace NAPS2.Util;

/// <summary>
/// Applies multiple mutations in order as a single mutation (and therefore a single undo step).
/// </summary>
public class CompoundListMutation<T>(params ListMutation<T>[] mutations) : ListMutation<T> where T : notnull
{
    public override void Apply(List<T> list, ref ListSelection<T> selection)
    {
        foreach (var mutation in mutations)
        {
            mutation.Apply(list, ref selection);
        }
    }
}
