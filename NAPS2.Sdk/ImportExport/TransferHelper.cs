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
        
        public static DirectImageTransfer GetImagesFromClipboard()
        {
            var data = Clipboard.Instance.GetData(ImageTypeName);
            return DirectImageTransfer.Parser.ParseFrom(data);
        }

        public static void SaveImagesToClipboard(ImageContext imageContext, IEnumerable<ScannedImage> images)
        {
            Clipboard.Instance.Clear();
            Clipboard.Instance.SetData(Images(imageContext, images).ToByteArray(), ImageTypeName);
        }

        public static bool ClipboardHasProfile() =>
            Clipboard.Instance.Contains(ProfileTypeName);

        public static DirectProfileTransfer GetProfileFromClipboard()
        {
            var data = Clipboard.Instance.GetData(ProfileTypeName);
            return DirectProfileTransfer.Parser.ParseFrom(data);
        }

        public static void SaveProfileToClipboard(ScanProfile profile) {
            Clipboard.Instance.Clear();
            Clipboard.Instance.SetData(Profile(profile).ToByteArray(), ProfileTypeName);
        }
    }
}
