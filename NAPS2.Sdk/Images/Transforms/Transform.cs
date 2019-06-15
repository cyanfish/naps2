using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Images.Storage;
using NAPS2.Serialization;

namespace NAPS2.Images.Transforms
{
    public abstract class Transform
    {
        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Transformers = new Dictionary<(Type, Type), (object, MethodInfo)>();
        
        /// <summary>
        /// Enumerates all methods on transformerObj that have a TransformerAttribute and registers them
        /// for future use in Transform.Perform and Transform.PerformAll with the specified image type.
        /// </summary>
        /// <param name="transformerObj"></param>
        public static void RegisterTransformers<TImage>(object transformerObj) where TImage : IImage
        {
            foreach (var method in transformerObj.GetType().GetMethods().Where(x => x.GetCustomAttributes(typeof(TransformerAttribute), true).Any()))
            {
                var methodParams = method.GetParameters();
                var storageType = methodParams[0].ParameterType;
                var transformType = methodParams[1].ParameterType;
                if (methodParams.Length == 2 &&
                    typeof(IImage).IsAssignableFrom(method.ReturnType) &&
                    storageType.IsAssignableFrom(typeof(TImage)) &&
                    typeof(Transform).IsAssignableFrom(transformType))
                {
                    Transformers[(typeof(TImage), transformType)] = (transformerObj, method);
                }
            }
        }

        // TODO: Describe ownership transfer
        /// <summary>
        /// Performs the specified transformation on the specified image using a compatible transformer.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Performs the specified transformations on the specified image using a compatible transformer.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static IImage PerformAll(IImage image, IEnumerable<Transform> transforms) => transforms.Aggregate(image, Perform);

        /// <summary>
        /// Appends the specified transform to the list, merging with the previous transform on the list if simplication is possible.
        /// </summary>
        /// <param name="transformList"></param>
        /// <param name="transform"></param>
        public static void AddOrSimplify(IList<Transform> transformList, Transform transform)
        {
            if (transform.IsNull)
            {
                return;
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

        // ReSharper disable once UnusedMember.Local
        private class TransformTypes : CustomXmlTypes<Transform>
        {
            protected override Type[] GetKnownTypes() => Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(Transform).IsAssignableFrom(t))
                .ToArray();
        }
    }
}
