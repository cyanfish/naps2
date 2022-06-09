using System.Collections.Immutable;

namespace NAPS2.Images.Transforms;

public abstract class Transform
{
    /// <summary>
    /// Appends the specified transform to the list, merging with the previous transform on the list if simplication is possible.
    /// </summary>
    public static ImmutableList<Transform> AddOrSimplify(ImmutableList<Transform> transformList, Transform transform)
    {
        if (transform.IsNull)
        {
            return transformList;
        }
        var last = transformList.LastOrDefault();
        if (last != null && transform.CanSimplify(last))
        {
            var simplified = transform.Simplify(last);
            if (simplified.IsNull)
            {
                return transformList.RemoveAt(transformList.Count - 1);
            }

            return transformList.SetItem(transformList.Count - 1, simplified);
        }

        return transformList.Add(transform);
    }
        
    /// <summary>
    /// Determines if this transform performed after another transform can be combined to form a single transform.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool CanSimplify(Transform other) => false;

    /// <summary>
    /// Combines this transform with a previous transform to form a single new transform.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual Transform Simplify(Transform other)
    {
        throw new InvalidOperationException();
    }

    /// <summary>
    /// Gets a value that indicates whether the transform is a null transformation (i.e. has no effect).
    /// </summary>
    public virtual bool IsNull => false;
}