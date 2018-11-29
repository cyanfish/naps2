using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Images.Storage;

namespace NAPS2.Images.Transforms
{
    public abstract class Transform
    {
        public static List<Type> KnownTransformTypes { get; } = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(Transform).IsAssignableFrom(t))
            .ToList(); private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Transformers = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public static void RegisterTransformers(Type imageType, object transformerObj)
        {
            if (!typeof(IImage).IsAssignableFrom(imageType))
            {
                throw new ArgumentException($"The image type must implement {nameof(IImage)}.", nameof(imageType));
            }
            foreach (var method in transformerObj.GetType().GetMethods().Where(x => x.GetCustomAttributes(typeof(TransformerAttribute), true).Any()))
            {
                var methodParams = method.GetParameters();
                var storageType = methodParams[0].ParameterType;
                var transformType = methodParams[1].ParameterType;
                if (methodParams.Length == 2 &&
                    typeof(IImage).IsAssignableFrom(method.ReturnType) &&
                    storageType.IsAssignableFrom(imageType) &&
                    typeof(Transform).IsAssignableFrom(transformType))
                {
                    Transformers.Add((imageType, transformType), (transformerObj, method));
                }
            }
        }

        public static IImage Perform(IImage image, Transform transform)
        {
            try
            {
                var (transformer, perform) = Transformers[(image.GetType(), transform.GetType())];
                return (IImage)perform.Invoke(transformer, new object[] { image, transform });
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"No transformer exists for {image.GetType().Name} and {transform.GetType().Name}");
            }
        }

        public static IImage PerformAll(IImage image, IEnumerable<Transform> transforms) => transforms.Aggregate(image, Perform);

        public static bool AddOrSimplify(IList<Transform> transformList, Transform transform)
        {
            if (transform.IsNull)
            {
                return false;
            }
            var last = transformList.LastOrDefault();
            if (transform.CanSimplify(last))
            {
                var simplified = transform.Simplify(last);
                if (simplified.IsNull)
                {
                    transformList.RemoveAt(transformList.Count - 1);
                }
                else
                {
                    transformList[transformList.Count - 1] = transform.Simplify(last);
                }
            }
            else
            {
                transformList.Add(transform);
            }
            return true;
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
}
