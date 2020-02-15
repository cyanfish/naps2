using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Images.Transforms;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.Images.Storage
{
    public abstract class ImageContext : IDisposable
    {
        private static ImageContext _default = new GdiImageContext();

        public static ImageContext Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly Dictionary<(Type, Type), (object, MethodInfo)> Transformers = new Dictionary<(Type, Type), (object, MethodInfo)>();

        protected ImageContext()
        {
            pdfRenderer = new PdfiumPdfRenderer(this);
        }

        /// <summary>
        /// Enumerates all methods on transformerObj that have a TransformerAttribute and registers them
        /// for future use in Transform.Perform and Transform.PerformAll with the specified image type.
        /// </summary>
        /// <param name="transformerObj"></param>
        public void RegisterTransformers<TImage>(object transformerObj) where TImage : IImage
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
        public IImage PerformTransform(IImage image, Transform transform)
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
        public IImage PerformAllTransforms(IImage image, IEnumerable<Transform> transforms) => transforms.Aggregate(image, PerformTransform);

        public void RegisterImageFactory<TImage>(IImageFactory factory) where TImage : IImage
        {
            ImageFactories[typeof(TImage)] = factory ?? throw new ArgumentNullException(nameof(factory));
        }
        
        public Type ImageType { get; protected set; }

        public void ConfigureBackingStorage<TStorage>() where TStorage : IStorage
        {
            BackingStorageType = typeof(TStorage);
        }

        public Type BackingStorageType { get; private set; } = typeof(IStorage);

        public IImageFactory ImageFactory => ImageFactories.Get(ImageType) ?? throw new InvalidOperationException($"No factory has been registered for the image type {ImageType.FullName}.");

        private readonly Dictionary<Type, IImageFactory> ImageFactories = new Dictionary<Type, IImageFactory>();

        public IImageMetadataFactory ImageMetadataFactory { get; set; } = new StubImageMetadataFactory();
        
        private readonly Dictionary<(Type, Type), (object, MethodInfo)> Converters = new Dictionary<(Type, Type), (object, MethodInfo)>();

        public void RegisterConverters(object converterObj)
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

        public IImage ConvertToImage(IStorage storage, StorageConvertParams convertParams)
        {
            if (storage is IImage image)
            {
                return image.Clone();
            }
            return (IImage)Convert(storage, ImageType, convertParams);
        }

        public IStorage ConvertToBacking(IStorage storage, StorageConvertParams convertParams)
        {
            return Convert(storage, BackingStorageType, convertParams);
        }

        public TStorage Convert<TStorage>(IStorage storage)
        {
            return (TStorage)Convert(storage, typeof(TStorage), new StorageConvertParams());
        }

        public TStorage Convert<TStorage>(IStorage storage, StorageConvertParams convertParams)
        {
            return (TStorage)Convert(storage, typeof(TStorage), convertParams);
        }

        public IStorage Convert(IStorage storage, Type type, StorageConvertParams convertParams)
        {
            if (type.IsInstanceOfType(storage))
            {
                if (storage is IImage image)
                {
                    return image.Clone();
                }
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
                throw new ArgumentException($"No converter exists from {storage.GetType().Name} to {type.Name}");
            }
        }

        public ScannedImage CreateScannedImage(IStorage storage)
        {
            return CreateScannedImage(storage, new StorageConvertParams());
        }

        public ScannedImage CreateScannedImage(IStorage storage, StorageConvertParams convertParams)
        {
            var backingStorage = ConvertToBacking(storage, convertParams);
            var metadata = ImageMetadataFactory.CreateMetadata(backingStorage);
            metadata.Commit();
            return new ScannedImage(backingStorage, metadata);
        }

        public ScannedImage CreateScannedImage(IStorage storage, IImageMetadata metadata, StorageConvertParams convertParams)
        {
            var backingStorage = ConvertToBacking(storage, convertParams);
            return new ScannedImage(backingStorage, metadata);
        }

        public ScannedImage CreateScannedImage(IStorage storage, BitDepth bitDepth, bool highQuality, int quality)
        {
            var convertParams = new StorageConvertParams { Lossless = highQuality, LossyQuality = quality, BitDepth = bitDepth };
            var backingStorage = ConvertToBacking(storage, convertParams);
            var metadata = ImageMetadataFactory.CreateMetadata(backingStorage);
            metadata.BitDepth = bitDepth;
            metadata.Lossless = highQuality;
            metadata.Commit();
            return new ScannedImage(backingStorage, metadata);
        }

        private FileStorageManager fileStorageManager = new FileStorageManager();

        public FileStorageManager FileStorageManager
        {
            get => fileStorageManager;
            set => fileStorageManager = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IPdfRenderer pdfRenderer;

        public IPdfRenderer PdfRenderer
        {
            get => pdfRenderer;
            set => pdfRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ImageContext UseFileStorage(string folderPath)
        {
            FileStorageManager = new FileStorageManager(folderPath);
            ImageMetadataFactory = new StubImageMetadataFactory();
            ConfigureBackingStorage<FileStorage>();
            return this;
        }

        public ImageContext UseFileStorage(FileStorageManager manager)
        {
            FileStorageManager = manager;
            ImageMetadataFactory = new StubImageMetadataFactory();
            ConfigureBackingStorage<FileStorage>();
            return this;
        }

        public ImageContext UseRecovery(string recoveryFolderPath)
        {
            var rsm = RecoveryStorageManager.CreateFolder(recoveryFolderPath);
            FileStorageManager = rsm;
            ImageMetadataFactory = rsm;
            ConfigureBackingStorage<FileStorage>();
            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                fileStorageManager.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class GdiImageContext : ImageContext
    {
        public GdiImageContext()
        {
            RegisterConverters(new GdiConverters(this));
            RegisterImageFactory<GdiImage>(new GdiImageFactory());
            RegisterTransformers<GdiImage>(new GdiTransformers());
            ImageType = typeof(GdiImage);
            // TODO: Not sure where to do these
            RegisterConverters(new PdfConverters(this));
        }
    }
}
