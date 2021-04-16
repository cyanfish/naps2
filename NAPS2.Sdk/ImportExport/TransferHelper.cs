using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eto.Forms;
using Google.Protobuf;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.ImportExport
{
    // TODO: Split
    // ProfileTransfer, ImageTransfer (injected helpers); ProfileTransferData, ImageTransferData (protos)
    // .SetClipboard(X); AddTo(DataObject); IsIn(DataObject); IsInClipboard(); GetFrom(DataObject); GetFromClipboard()
    // We can have a generic base class (TransferHelper) that implements the clipboard methods.
    // ImageContext is injected for imagetransfer
    public static class TransferHelper
    {
        public static string ImageTypeName { get; } = "NAPS2.Serialization.DirectImageTransferProto";
        public static string ProfileTypeName { get; } = "NAPS2.Serialization.DirectProfileTransferProto";

        public static DirectImageTransfer Images(ImageContext imageContext, IEnumerable<ScannedImage> images)
        {
            var transfer = new DirectImageTransfer
            {
                ProcessId = Process.GetCurrentProcess().Id
            };
            var serializedImages = images.Select(x => SerializedImageHelper.Serialize(imageContext, x, new SerializedImageHelper.SerializeOptions()));
            transfer.SerializedImages.AddRange(serializedImages);
            return transfer;
        }
        
        public static DirectProfileTransfer Profile(ScanProfile profile)
        {
            var profileCopy = profile.Clone();
            profileCopy.IsDefault = false;
            profileCopy.IsLocked = false;
            profileCopy.IsDeviceLocked = false;
            return new DirectProfileTransfer
            {
                ProcessId = Process.GetCurrentProcess().Id,
                ScanProfileXml = profileCopy.ToXml(),
                Locked = profile.IsLocked
            };
        }

        public static bool ClipboardHasImages() =>
            Clipboard.Instance.Contains(ImageTypeName);

        public static DirectImageTransfer GetImagesFromClipboard() => GetImagesFromDataObject(Clipboard.Instance);
        
        public static DirectImageTransfer GetImagesFromDataObject(IDataObject dataObject)
        {
            var data = dataObject.GetData(ImageTypeName);
            return DirectImageTransfer.Parser.ParseFrom(data);
        }

        public static void SaveImagesToClipboard(ImageContext imageContext, IEnumerable<ScannedImage> images)
        {
            Clipboard.Instance.Clear();
            SaveImagesToDataObject(imageContext, images, Clipboard.Instance);
        }
        
        public static void SaveImagesToDataObject(ImageContext imageContext, IEnumerable<ScannedImage> images, IDataObject dataObject) {
            dataObject.SetData(Images(imageContext, images).ToByteArray(), ImageTypeName);
        }

        public static bool ClipboardHasProfile() =>
            Clipboard.Instance.Contains(ProfileTypeName);
        
        public static bool HasProfile(IDataObject dataObject) =>
            dataObject.Contains(ProfileTypeName);

        public static DirectProfileTransfer GetProfileFromClipboard() => GetProfileFromDataObject(Clipboard.Instance);
        
        public static DirectProfileTransfer GetProfileFromDataObject(IDataObject dataObject)
        {
            var data = dataObject.GetData(ProfileTypeName);
            return DirectProfileTransfer.Parser.ParseFrom(data);
        }

        public static void SaveProfileToClipboard(ScanProfile profile) {
            Clipboard.Instance.Clear();
            SaveProfileToDataObject(profile, Clipboard.Instance);
        }
        
        public static void SaveProfileToDataObject(ScanProfile profile, IDataObject dataObject) {
            dataObject.SetData(Profile(profile).ToByteArray(), ProfileTypeName);
        }
    }
}
