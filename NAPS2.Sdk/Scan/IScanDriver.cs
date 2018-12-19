using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;

namespace NAPS2.Scan
{
    /// <summary>
    /// An interface for document scanning drivers (WIA, TWAIN, SANE).
    /// </summary>
    public interface IScanDriver
    {
        /// <summary>
        /// Gets a value indicating whether the driver is supported on the current platform.
        /// </summary>
        bool IsSupported { get; }
        
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
        ScanDevice PromptForDevice(ScanProfile scanProfile = null, IntPtr dialogParent = default);

        /// <summary>
        /// Gets a list of available scanning devices.
        /// </summary>
        /// <returns>The list of devices.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs when reading the available devices.</exception>
        List<ScanDevice> GetDeviceList(ScanProfile scanProfile = null);

        /// <summary>
        /// Scans one or more images, interacting with the user as necessary.
        /// </summary>
        /// <returns>A list of scanned images.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs while scanning.</exception>
        /// /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if ScanProfile or DialogParent has not been set.</exception>
        ScannedImageSource Scan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default);
    }
}
