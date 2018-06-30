using NAPS2.Scan.Images;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NAPS2.Scan
{
    /// <summary>
    /// An interface for document scanning drivers (e.g. WIA, TWAIN).
    /// </summary>
    public interface IScanDriver
    {
        /// <summary>
        /// Sets the profile used by the driver for scanning.
        /// This must be set before calling Scan.
        /// </summary>
        ScanProfile ScanProfile { set; }

        /// <summary>
        /// Sets the runtime parameters used by the driver for scanning.
        /// This must be set before calling Scan.
        /// </summary>
        ScanParams ScanParams { set; }

        /// <summary>
        /// Sets the device used by the driver for scanning.
        /// This must be set before calling Scan.
        /// </summary>
        ScanDevice ScanDevice { set; }

        /// <summary>
        /// Sets the parent window used when creating dialogs. This must be set before calling PromptForDevice or Scan.
        /// </summary>
        IWin32Window DialogParent { set; }

        /// <summary>
        /// Gets the name used to look up the driver in the IScanDriverFactory.
        /// </summary>
        string DriverName { get; }

        /// <summary>
        /// Prompts the user (via a dialog) to select a scanning device.
        /// </summary>
        /// <returns>The selected device, or null if no device was selected.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs when reading the available devices.</exception>
        /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if DialogParent has not been set.</exception>
        ScanDevice PromptForDevice();

        /// <summary>
        /// Gets a list of available scanning devices.
        /// </summary>
        /// <returns>The list of devices.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs when reading the available devices.</exception>
        List<ScanDevice> GetDeviceList();

        /// <summary>
        /// Scans one or more images, interacting with the user as necessary.
        /// </summary>
        /// <returns>A list of scanned images.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs while scanning.</exception>
        /// /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if ScanProfile or DialogParent has not been set.</exception>
        IEnumerable<ScannedImage> Scan();
    }
}