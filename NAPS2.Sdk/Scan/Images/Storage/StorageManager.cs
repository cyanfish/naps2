using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images.Storage
{
    public static class StorageManager
    {
        // TODO: Maybe move the initialization to Lib.Common

        public static Type PreferredBackingStorageType { get; set; } = typeof(FileStorage);

        public static Type PreferredMemoryStorageType { get; set; } = typeof(GdiStorage);

        public static HashSet<Type> BackingStorageTypes { get; set; } = new HashSet<Type> { typeof(FileStorage), typeof(PdfFileStorage) };

        public static IMemoryStorageFactory MemoryStorageFactory { get; set; } = new GdiStorageFactory();

        public static IImageMetadataFactory ImageMetadataFactory { get; set; }

        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Transformers = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public static void RegisterTransformer(object transformer)
        {
            foreach (var interfaceType in transformer.GetType().GetInterfaces().Where(x => x.Name == "ITransformer"))
            {
                var genericArgs = interfaceType.GetGenericArguments();
                Transformers.Add((genericArgs[0], genericArgs[1]), (transformer, interfaceType.GetMethod("PerformTransform")));
            }

        }

        public static IMemoryStorage PerformTransform(IMemoryStorage storage, Transform transform)
        {
            try
            {
                var (transformer, perform) = Transformers[(storage.GetType(), transform.GetType())];
                return (IMemoryStorage)perform.Invoke(transformer, new object[] { storage, transform });
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"No transformer exists for {storage.GetType().Name} and {transform.GetType().Name}");
            }
        }

        public static IMemoryStorage PerformAllTransforms(IMemoryStorage storage, IEnumerable<Transform> transforms)
        {
            return transforms.Aggregate(storage, PerformTransform);
        }

        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Converters = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public static void RegisterConverter<TStorage1, TStorage2>(IStorageConverter<TStorage1, TStorage2> converter) where TStorage1 : IStorage where TStorage2 : IStorage
        {
            Converters.Add((typeof(TStorage1), typeof(TStorage2)), (converter, typeof(IStorageConverter<TStorage1, TStorage2>).GetMethod("Convert")));
        }

        public static IStorage ConvertToBacking(IStorage storage, StorageConvertParams convertParams)
        {
            if (BackingStorageTypes.Contains(storage.GetType()))
            {
                return storage;
            }
            return Convert(storage, PreferredBackingStorageType, convertParams);
        }

        public static IMemoryStorage ConvertToMemory(IStorage storage, StorageConvertParams convertParams)
        {
            if (storage is IMemoryStorage memStorage)
            {
                return memStorage;
            }
            return (IMemoryStorage)Convert(storage, PreferredMemoryStorageType, convertParams);
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
            var gdiFileConverter = new GdiFileConverter(new FileStorageManager());
            RegisterConverter<GdiStorage, FileStorage>(gdiFileConverter);
            RegisterConverter<FileStorage, GdiStorage>(gdiFileConverter);
            var gdiTransformer = new GdiTransformer();
            RegisterTransformer(gdiTransformer);
        }
    }
}
