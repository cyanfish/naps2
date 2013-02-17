using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS.Scan.Driver
{
    /// <summary>
    /// An interface for document scanning drivers (e.g. WIA, TWAIN).
    /// </summary>
    public interface IScanDriver
    {
        /// <summary>
        /// Sets the settings used by the driver for scanning.
        /// This must be set before calling Scan.
        /// Some drivers may check for particular implementors and use further information than provided by IScanSettings.
        /// </summary>
        IScanSettings ScanSettings { set; }

        /// <summary>
        /// Sets the parent window used when creating dialogs. This must be set before calling PromptForDevice or Scan.
        /// </summary>
        IWin32Window DialogParent { set; }

        /// <summary>
        /// Prompts the user (via a dialog) to select a scanning device.
        /// </summary>
        /// <returns>The selected device, or null if no device was selected.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs when reading the available devices.</exception>
        /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if DialogParent has not been set.</exception>
        IScanDevice PromptForDevice();

        /// <summary>
        /// Scans one or more images, interacting with the user as necessary.
        /// </summary>
        /// <returns>A list of scanned images.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs while scanning.</exception>
        /// /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if ScanSettings or DialogParent has not been set.</exception>
        List<IScannedImage> Scan();
    }
}
