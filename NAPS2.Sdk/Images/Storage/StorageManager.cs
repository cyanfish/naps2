using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Images.Transforms;

namespace NAPS2.Images.Storage
{
    public static class StorageManager
    {
        // TODO: Maybe move the initialization to Lib.Common

        public static Type PreferredBackingStorageType { get; set; } = typeof(FileStorage);

        public static Type PreferredImageType { get; set; } = typeof(GdiImage);

        public static HashSet<Type> BackingStorageTypes { get; set; } = new HashSet<Type> { typeof(FileStorage), typeof(PdfFileStorage) };

        public static IImageFactory ImageFactory { get; set; } = new GdiImageFactory();

        public static IImageMetadataFactory ImageMetadataFactory { get; set; }

        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Transformers = new Dictionary<(Type, Type), (object, MethodInfo)>();

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

        public static IImage PerformTransform(IImage image, Transform transform)
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

        public static IImage PerformAllTransforms(IImage image, IEnumerable<Transform> transforms)
        {
            return transforms.Aggregate(image, PerformTransform);
        }

        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Converters = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public static void RegisterConverters(object converterObj)
        {
            foreach (var method in converterObj.GetType().GetMethods().Where(x => x.GetCustomAttributes(typeof(StorageConverterAttribute), true).Any()))
            {
                var methodParams = method.GetParameters();
                var inputType = methodParams[0].ParameterType;
                var outputType = method.ReturnType;
                var paramsType = methodParams[1].ParameterType;
                if (methodParams.Length == 2 &&
                    typeof(IStorage).IsAssignableFrom(inputType) &&
                    typeof(IStorage).IsAssignableFrom(outputType) &&
                    paramsType == typeof(StorageConvertParams))
                {
                    Converters.Add((inputType, outputType), (converterObj, method));
                }
            }
        }

        public static IStorage ConvertToBacking(IStorage storage, StorageConvertParams convertParams)
        {
            if (BackingStorageTypes.Contains(storage.GetType()))
            {
                return storage;
            }
            return Convert(storage, PreferredBackingStorageType, convertParams);
        }

        public static IImage ConvertToImage(IStorage storage, StorageConvertParams convertParams)
        {
            if (storage is IImage memStorage)
            {
                return memStorage;
            }
            return (IImage)Convert(storage, PreferredImageType, convertParams);
        }

        public static TStorage Convert<TStorage>(IStorage storage)
        {
            return (TStorage)Convert(storage, typeof(TStorage), new StorageConvertParams());
        }

        public static TStorage Convert<TStorage>(IStorage storage, StorageConvertParams convertParams)
        {
            return (TStorage)Convert(storage, typeof(TStorage), convertParams);
        }

        public static IStorage Convert(IStorage storage, Type type, StorageConvertParams convertParams)
        {
            if (storage.GetType() == type)
            {
                return storage;
            }
            // TODO: Dispose old storage? Consider ownership. Possibility: Clone/Dispose ref counts.
            try
            {
                var (converter, convert) = Converters[(storage.GetType(), type)];
                return (IStorage)convert.Invoke(converter, new object[] { storage, convertParams });
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"No converter exists from {storage.GetType().Name} to {PreferredBackingStorageType.Name}");
            }
        }

        static StorageManager()
        {
            RegisterConverters(new GdiConverters());
            RegisterTransformers(typeof(GdiImage), new GdiTransformers());
        }
    }
}
