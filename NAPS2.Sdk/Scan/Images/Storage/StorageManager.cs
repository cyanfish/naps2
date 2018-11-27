using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static readonly Dictionary<(Type, Type), (object, MethodInfo)> Converters = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public static void RegisterConverter<TStorage1, TStorage2>(IStorageConverter<TStorage1, TStorage2> converter)
        {
            Converters.Add((typeof(TStorage1), typeof(TStorage2)), (converter, typeof(IStorageConverter<TStorage1, TStorage2>).GetMethod("Convert")));
        }

        public static IStorage ConvertToBacking(IStorage storage)
        {
            if (BackingStorageTypes.Contains(storage.GetType()))
            {
                return storage;
            }
            return Convert(storage, PreferredBackingStorageType);
        }

        public static IStorage ConvertToMemory(IStorage storage)
        {
            if (storage is IMemoryStorage)
            {
                return storage;
            }
            return Convert(storage, PreferredMemoryStorageType);
        }

        public static IStorage Convert(IStorage storage, Type type)
        {
            // TODO: Dispose old storage?
            try
            {
                var (converter, convert) = Converters[(storage.GetType(), type)];
                return (IStorage) convert.Invoke(converter, new object[] {storage});
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
        }
    }
}
