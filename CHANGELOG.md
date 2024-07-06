Changes in 7.4.3:
- Fixed some ESCL connection issues
- Fixed email compatibility with HCL Notes
- Fixed issues with "Outlook Web Access" email provider
- Fixed SANE compability with some HP devices
- Fixed Fraktur-based languages for OCR

Changes in 7.4.2:
- Bug fixes

Changes in 7.4.1:
- Improved OCR text alignment
- Added "Open With" support for PDF and image files
- Changed some labels to improve clarity
  - "Automatically run OCR after scanning" &rarr; "Pre-emptively run OCR after scanning"
  - "Flip duplexed pages" &rarr; "Flip back sides of duplex pages"
- Added HTTPS support for scanner sharing
  - Uses an auto-generated self-signed certificate by default
  - Admins can set [EsclServerCertificatePath](https://www.naps2.com/doc/org-use#escl-server-certificate-path) to use a custom certificate
  - Admins can set [EsclSecurityPolicy](https://www.naps2.com/doc/org-use#escl-security-policy) to force servers/clients to only use HTTPS
    - This affects all ESCL devices, not just shared scanners
- Improved ESCL reliability with network interruptions
- Fixed some issues with Preview window zooming
- Made confirmation dialogs more consistent (OK/Cancel vs. Yes/No)
- Added more default keyboard shortcuts
- Mac: Fixed issues with keyboard shortcuts
- Mac: Added some missing menu items (Zoom In/Out, Move Up/Down, Profiles)
- Linux: Added a signature to .deb/.rpm packages
- Windows: The .msi installer can no longer be used to upgrade over .exe
- Bug fixes

Changes in 7.4.0:
- Added Undo/Redo (from the right-click menu or Ctrl+Z)
  - Deletions can't be undone 
- Added Split/Combine (under the Image menu)
  - Split can be used for book scanning to separate pages
  - Combine can be used to include front/back sides of an ID card in one image
- Added "Multiple Languages" as an option for OCR (in the OCR language dropdown)
- Added a "Fix white balance and remove noise" OCR option
  - This can improve OCR with low-quality scans, but will make OCR slower
  - This is equivalent to using "Document Correction" before OCR
- Upgraded Tesseract from 5.2.0 to 5.3.4 for OCR
- Added a "Show native TWAIN progress" profile option (under Advanced) 
- Bug fixes

Changes in 7.3.1:
- Improved loading time for "Keep images across sessions"
- PDF encryption settings are now hidden until enabled
- Fixed some SANE devices incorrectly appearing offline
- Fixed some SANE devices not respecting page size
- Fixed OCR issues with non-Latin alphabets
- Bug fixes

Changes in 7.3.0:
- Added a general "Settings" window with new options (some not available on Mac/Linux):
  - Show page numbers
  - Show Profiles toolbar
  - Scan menu changes default profile
  - Scan button default action
  - Save button default action
  - Clear images after saving
  - Keep images across sessions
  - Only allow a single NAPS2 instance
- Added corresponding appsettings.xml options
  - See https://www.naps2.com/doc/org-use
- Added "mode" attribute to some settings in appsettings.xml:
  - mode="default" provides a default value for the user
  - mode="lock" prevents the user from changing the value
- Added new console options:
  - "--noprofile" to only use CLI options (not GUI profiles)
  - "--listdevices" to see available scanning devices
  - "--driver", "--device", "--source", "--pagesize", "--dpi", "--bitdepth" scanning options
  - "--deskew", "--rotate" post-processing options
  - See https://www.naps2.com/doc/command-line
- Windows: Updated .exe installer style
- Windows: Added back "Alt" hotkeys
- Windows: Fixed an issue sending email with Outlook 2010-2016
- Bug fixes

Changes in 7.2.2:
- Bug fixes

Changes in 7.2.1:
- Bug fixes

Changes in 7.2.0:
- Scanner Sharing
  - Share scanners with other computers on the local network, for example:
    - Turn a desktop-connected USB scanner into a wireless scanner usable from your laptop or phone
    - Allow Windows-only scanners to be used from Mac/Linux using a virtual machine
    - Set up a Raspberry Pi to turn a USB scanner into a wireless scanner
  - On the host computer, in the Profiles window, click Scanner Sharing and choose the scanners to share
  - On the client computer, select "ESCL Driver" in your profile settings and you should be able to select the shared scanner
  - NAPS2 currently must be kept open on the host for sharing to work
  - Shared scanners can be used from any ESCL-capable client, not just NAPS2
    - Try [Mopria Scan](https://play.google.com/store/apps/details?id=org.mopria.scan.application) for Android
  - Use NoScannerSharing in appsettings.xml to disable
- Slightly updated icons in the Profiles window 
- Old unrecoverable files are now cleaned up on startup
- Mac/Linux have been upgraded to the .NET 8 runtime
- Linux flatpak runtime has been upgraded to 23.08
- Bug fixes

Changes in 7.1.2:
- Mac: Fixed scanning with macOS 14 Sonoma 

Changes in 7.1.1:
- Bug fixes

Changes in 7.1.0:
- Windows: Added ESCL Driver option
  - This allows using most network scanners without needing to install a separate driver
- PDF saving is much faster in some cases
- Imported PDFs now render forms and annotations 
- Added Hindi language 
- Bug fixes
- NAPS2.Sdk is now available on [Nuget](https://www.nuget.org/packages/NAPS2.Sdk)

Changes in 7.0b9:
- Improved accuracy of PDF page sizes
- Improved UI responsiveness when OCR is running 
- Mac: Improved color accuracy for scans with Apple Driver
- Mac: Added support for dark themes
- Linux: Added support for dark themes
- Linux: Added arm64 .deb/.rpm builds
- Bug fixes

Changes in 7.0b8:
- Added "Email PDF" support to Mac and Linux
  - Mac: Apple Mail, Gmail, and Outlook Web options
  - Linux: Thunderbird, Gmail, and Outlook Web options
- Added "Print" support to Mac and Linux 
- Added notifications to Mac and Linux
  - Also updated notification appearance in general
- Linux: Added drag & drop support
- Linux: Improved compatibility with older Linux (e.g. Ubuntu 18.04)
- Linux: Added dependencies to .deb package  
- Sane: Show IP addresses for escl/airscan backends
- Windows: Changed installer publisher to "NAPS2 Software"
- Improved error log formatting
- Added debug logging for scanning diagnostics
  - Turn on by checking "Enable debug logging" in the About window
  - This will record information about scanning activity on disk
  - You can find debuglog.txt in the [same folder](https://www.naps2.com/doc/troubleshooting#error-log) as errorlog.txt
  - Use NoDebugLogging in appsettings.xml to hide the option
- Added Bosnian and Indonesian languages
- Bug fixes

Changes in 7.0b7:
- Bug fixes

Changes in 7.0b6:
- Bug fixes

Changes in 7.0b5:
- Added 2400/4800 dpi options
- Linux: Added .deb/.rpm packages
- Sane: Show devices incrementally (only with Mac / Linux flatpak)
- Crop improvements
- Fixed formatting for OCR of non-NAPS2 PDFs
- Bug fixes

Changes in 7.0b4:
- Twain: Changed default transfer mode
  - "Alternative Transfer" has been renamed "Memory Transfer" and is now used when "Default" is selected
  - "Native Transfer" can be used to revert to the old transfer mode
- Saved images now use optimized bit depths for smaller file sizes
- Bug fixes 

Changes in 7.0b3:
- Bug fixes

Changes in 7.0b2:
- Bug fixes

Changes in 7.0b1:
- Most NAPS2 code has been rewritten. Things should mostly look the same but under the hood there are many differences.
    - [Beta feedback thread](https://github.com/cyanfish/naps2/discussions/35)
- Added Mac support
    - Supports macOS 10.15 and later
    - The Universal download should work for all users. Or you can use the Intel/Apple Silicon downloads for a smaller download/install size if you know which one your Mac has.
    - NAPS2 on Mac bundles SANE drivers for USB scanners, allowing supported scanners to be used even on new M1/M2 Macs (which normally wouldn't work without manufacturer-provided drivers)
- Added native Linux support
    - Requires Flatpak for installation (https://flatpak.org/setup/)
    - Mono is no longer required
    - The UI should now feel like a native Linux app
    - Much better performance and reliability
- TWAIN support has been reworked
    - Some lifecycle-related issues should hopefully be fixed (e.g. only being able to scan once)
    - With "Use predefined settings", TWAIN now uses the built-in NAPS2 progress window, which allows multitasking
    - TWAIN UI should no longer be visible in console and batch mode
    - TWAIN should also now support scanning larger images (e.g. 1200dpi) without out-of-memory issues
- Upgraded Tesseract to 5.2.0 for OCR
    - Up to 30% faster OCR performance
    - Tesseract is now bundled with the NAPS2 download, so no extra download is required (though you still need to download language data if you don't already have it).
- PDF import and export have been rewritten to leverage Pdfium
    - This means better support for importing different kinds of PDFs
    - In some cases this means much faster import/export
    - Pdfium is bundled with the NAPS2 download so there is no longer an extra download needed to import non-NAPS2 PDFs
- New Crop UI
- Minor tweaks to blank page detection
- Image list tweaks
    - Selected images appear with just a blue border
    - Spacing has been optimized
- New automatic image correction functionality (work in progress)
    - "Document Correction" under the Image menu
    - Automatic fixing of color calibration, noise, skew, and other common scanning issues
    - Eventually this will be integrated into profiles
- JPEG2000 support for importing/saving images (Mac only for now)
- Dropped support for rarely-used image file formats (.emf, .exif, .gif)
    - Please request if you want this back
- NAPS2 on Windows now requires .NET Framework 4.6.2
    - This means no more support for Windows XP
    - Windows 7 SP1 is now the minimum requirement
- The 64-bit Windows install location is now "Program Files" instead of "Program Files (x86)"
- The MSI installer now has separate 64-bit and 32-bit downloads
- The AppData format for config.xml and Tesseract files has changed (will be automatically migrated)
- Improved icon quality
- Translations have been moved to Crowdin
    - See [translate.naps2.com](https://translate.naps2.com)
- Various performance and reliability improvements
- Bug fixes

Changes in 6.1.2:
- Added --autosend support for Gmail in NAPS2.Console
- Bug fixes

Changes in 6.1.1:
- Faster and more accurate deskew
- Bug fixes

Changes in 6.1.0:
- Added a "Single page files" option in PDF Settings
- Improved accessibility
- Faster cropping
- Event logging now uses an XML format
- Bug fixes

Changes in 6.0b4:
- Beta feedback thread: https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- Upgraded WIA version from 1.0 to 2.0; can be changed back in your profile under Advanced
- Improved WIA compatibility with feeders and duplex
- Added support for background scanning with WIA
    - Does not work with "Use native UI"
    - This means you can scan with multiple devices at the same time
- Removed some obsolete WIA compatibility options
- Bug fixes

Changes in 6.0b3:
- Beta feedback thread: https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- Added optional event logging
    - See https://www.naps2.com/doc-org-use.html#event-logging
- Improved console import speed
- Bug fixes

Changes in 6.0b2:
- Beta feedback thread: https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- OCR users from 6.0b1 will need to click the OCR button and re-download
- Fixed an issue with OCR missing a DLL on some systems
- Fixed an issue with OCR not terminating
- Other minor fixes and improvements

Changes in 6.0b1:
- Beta feedback thread: https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- Linux support (download one of the portable archives - currently experimental, please give feedback!)
    - Requires Mono (5.17+ preferably), see https://www.naps2.com/doc-getting-started.html#system-requirements
- Added an automatic update check
    - Opt in from the About window
    - Not available if installed from the MSI
- New OCR version, significantly more accurate in many cases
    - The OCR button will prompt to update. This can be disabled with the NoUpdatePrompt flag in appsettings.xml
    - Not supported on Windows XP (will use the older version instead)
    - You can choose between multiple modes: Fast (recommended), Best (slow), and Legacy (to simulate the older version)
- Added the ability to choose an email provider
    - When you first click Email PDF, you will be prompted to choose. Afterwards use Email Settings to change
    - Switch between installed clients (Outlook, Thunderbird, etc.)
    - Webmail integration for Gmail and Outlook Web Access
- Added support for Unicode in email attachment names
- Crop selection will be remembered (in case you're cropping multiple images but need to adjust them individually)
- Added the ability to run most operations in the background for multitasking
- Improved performance with very large images
- Substantially reduced installation footprint and portable zip size
- Minimized TWAIN UI in console and batch mode
- NAPS2 installers are now signed
    - This should eventually help remove SmartScreen notifications
- NAPS2 will now run in 64-bit mode on compatible systems
    - If you have a 64-bit system, NAPS2 will better handle memory-intensive operations
    - If you downloaded the add-on to open any PDF (gsdll32.dll), you may need to re-download the 64-bit version
- Improved documentation and usability for developers (see https://www.naps2.com/doc-dev-onboarding.html)
- Bug fixes

Changes in 5.8.2:
- Added Japanese language
- Fixed a bug with importing some PDFs
- Fixed a bug with the Alternative Transfer TWAIN option

Changes in 5.8.1:
- Fixed a bug with PDF/A support

Changes in 5.8.0:
- PDF/A support
    - PDF/A1-b, PDF/A2-b, PDF/A3-b, and PDF/A3-u support
    - In the "Save PDF" menu, click "PDF Settings", and select it under "Compatibility"
    - Use --pdfcompat in NAPS2.Console. See www.naps2.com/doc-command-line.html#pdf-options
    - Use ForcePdfCompat in appsettings.xml. See www.naps2.com/doc-org-use.html#force-pdf-compat
- TIFF changes
    - Better compression for black and white TIFF files by default
    - Added a "Compression" option under Image Settings
    - Added a "Single page files" option under Image Settings that prevents saving multi-page TIFF files
    - Use --tiffcomp and --split in NAPS2.Console. See www.naps2.com/doc-command-line.html#image-options
- Donate button
    - The About window now has a Donate button
    - An unobtrusive donation prompt is shown after a month of use
    - Use HideDonateButton in appsettings.xml to disable both. See www.naps2.com/doc-org-use.html#hide-donate-button
    - The prompt is disabled by default in the MSI distribution
- Added multi-language support to the EXE installation wizard

Changes in 5.7.1:
- Added --split, --splitscans, --splitpatcht, and --splitsize options to NAPS2.Console
    - See www.naps2.com/doc-command-line.html#split-options
- Added slice support to --import in NAPS2.Console
    - See www.naps2.com/doc-command-line.html#slicing-imported-files

Changes in 5.7.0:
- Fixed downloads for OCR (etc.)
- Improved deskew
- Added a confirmation for batch cancel
- Minor performance improvements
- Bug fixes

Changes in 5.6.2:
- Bug fixes

Changes in 5.6.1:
- Fixed a crash

Changes in 5.6.0:
- Increased the maximum thumbnail size from 256x256 to 1024x1024
- Improved PDF import to allow many more types of PDFs to be imported
- OCR can now be used on imported PDFs (if they don't already have text)
- Improved PDF file size for some black and white images
- Combined Brightness and Contrast adjustments into a single window
- Added Hue, Saturation, Black+White, and Sharpen image adjustments
- Added more keyboard shortcuts in the preview window (arrow keys to change pages, Ctrl/Alt/Shift + arrow keys to pan)
- Added "HideImportButton", "HideOcrButton", "HideSavePdfButton", and "HideSaveImages" options to appsettings.xml
- Added "OcrState" and "OcrDefaultLanguage" options to appsettings.xml
- Bug fixes

Changes in 5.5.0:
- Added support for importing any PDF (requires an additional download, can be disabled by NoUpdatePrompt or DisableGenericPdfImport in appsettings.xml)
- Added the ability to install optional components using NAPS2.Console (with the "--install" argument)
- Added "Alternative Transfer" TWAIN compatibility option
- Added .txt extension to license/contributor file names
- Bug fixes

Changes in 5.4.0:
- Added automatic deskew option (under the Rotate menu or under Advanced in your profile settings) (credit to Peter Hommel)
- Added single-page save buttons to the preview window
- Added "Prompt for file path" option to Auto Save Settings
- Split "Force matching page size" option into "Stretch to page size" and "Crop to page size" options
- Added "Retry on failure" and "Delay between scans" WIA compatibility options
- Added support for environment variables in most paths
- Added LICENSE and CONTRIBUTORS files to the root directory (this replaces most copyright notices elsewhere)
- Added Nynorsk language
- Bug fixes

Changes in 5.3.3:
- Bug fixes

Changes in 5.3.2:
- Added Slovenian language
- Fixed AV false positive issue

Changes in 5.3.1:
- Added Afrikaans and Vietnamese languages

Changes in 5.3.0:
- Significantly improved OCR speed on multi-core systems
- Improved OCR text alignment
- Patch-T is now supported for all scanners, with both WIA and TWAIN
- Improved and added technical details to some error messages
- Tweaked the spacing between thumbnails for less wasted space
- Added Latvian language
- Fixed OCR on Windows XP (requires an extra download, can be disabled by NoUpdatePrompt in appsettings.xml)
- Fixed Auto Save and Batch to use a default file name when a directory is specified instead of a file path

Changes in 5.2.1:
- Added an "OcrTimeoutInSeconds" option to appsettings.xml
- Bug fixes

Changes in 5.2.0:
- Added the ability to copy/paste and drag/drop profiles
- Changed the way "LockSystemProfiles" behaves to allow users to specify a device if not specified by the admin
- Added "NoUserProfiles", "AlwaysRememberDevice", and "LockUnspecifiedDevices" options to appsettings.xml
- Added "HideEmailButton" and "HidePrintButton" options to appsettings.xml
- Added "PromptIfSelected" as a possible value for the "SaveButtonDefaultAction" option in appsettings.xml
- Added Arabic, Serbian (Latin + Cyrillic), and Slovak languages

Changes in 5.1.1:
- Updated the default appsettings.xml to be easier to edit
- Bug fixes

Changes in 5.1.0:
- Custom page sizes can now be named and reused across multiple profiles
- Added the ability to draw a line to align the page in Custom Rotation
- Added a "Restore Defaults" button to Advanced Profile Settings
- Added a "ComponentsPath" option to appsettings.xml
- Added a "SingleInstance" option to appsettings.xml
- Placeholders can now be used in --subject and --body arguments in NAPS2.Console
- Bug fixes

Changes in 5.0b3:
- Added save notifications (use DisableSaveNotifications in appsettings.xml to disable)
- Added a "Skip save prompt" option to PDF and Image settings. Also changed "Default File Name" to "Default File Path" (can be a file name, folder, or full path now)
- Bug fixes

Changes in 5.0b2:
- Added a "Flip duplexed pages" compatibility option
- Added a "DeleteAfterSaving" option to appsettings.xml
- Bug fixes

Changes in 5.0b1:
- Updated tesseract-ocr (from 3.02 to 3.04)
    - The OCR button will prompt to update. This can be disabled with the NoUpdatePrompt flag in appsettings.xml
    - If you have the old version it will continue to function normally
- Updated the default TWAIN implementation
    - Choose the "Old DSM" implementation under advanced profile settings to revert
- Changed the default Horizontal Align in profile settings from Left to Right to match most scanners
    - If you deploy your own appsettings.xml the specified alignment specified will continue to be used as default
- Added a "LockSystemProfiles" flag to appsettings.xml that allows an administrator better control over user profiles
    - See www.naps2.com/doc-org-use.html#lock-system-profiles
- Added an "Offset width based on alignment (WIA)" compatibility option (for ticket #124)
- Added Farsi and Korean languages to installers

Changes in 4.7.2:
- Fixed a TWAIN issue

Changes in 4.7.1:
- Improved memory capabilities on 64-bit systems
- Fixed a WIA issue

Changes in 4.7.0:
- Added option in NAPS2.Console to use auto-save settings (-a/--autosave)
- Added click-and-drag scrolling in the preview window
- Improved cropping (can now click and drag to select an area)
- Added more descriptive error messages for some WIA errors (e.g. device busy)
- Fixed button alignment on left/right toolbar placements
- Added Korean, Lithuanian, and Farsi languages
- Various performance improvements
- Various bug fixes

Changes in 4.6.1:
- Bug fixes

Changes in 4.6.0:
- New feature: Exclude blank pages (under "Advanced" in profile settings)
- New options in NAPS2.Console for reordering (e.g. interleave)
- Keyboard shortcuts are now customizable in appsettings.xml (and some more default shortcuts added)
- Optional file type filters when importing
- Importing multiple files at once now sorts the files better
- Fix an issue with the left side of the scanned page being cut off with WIA
- Other bug fixes

Changes in 4.5.1:
- Improved performance when editing and rearranging thumbnails
- Automatically scroll the thumbnail list when trying to drag thumbnails up or down
- Display an indicator while dragging thumbnails to show where they'll drop
- Fixed Thai/Tagalog OCR language download
- Fixed minor translation issues

Changes in 4.5.0:
- New feature: Auto Save - Enable it from the profile editor (can be disabled by organizations in appsettings.xml)
- New feature: Drag and Drop support (re-order images within NAPS2, import files into NAPS2, or copy images between different instances of NAPS2)
- New feature: "Advanced" profile options for image quality and scanner compatibility
- New feature: Copy/Paste within NAPS2 (previously could only copy, not paste)
- New progress dialogs for Import, Save, etc. with cancellation
- Better contrast implementation
- Selected images are now kept in view when editing and reordering images
- The default action when clicking on Save PDF, Save Images, and Email PDF can be configured in appsettings.xml (SaveAll, SaveSelected, or AlwaysPrompt)
- New command-line options for NAPS2.exe to enable/disable scanning from a physical "Scan" button in portable versions ("/RegisterSti", "/UnregisterSti", and "/Silent")
- Improved TWAIN error logging
- Bug fixes

Changes in 4.4.1:
- Tool strip location in the main form is remembered
- Bug fixes

Changes in 4.4.0:
- New feature: NAPS2 can be started and/or instantly scan when you press the physical "Scan" button on your scanner (requires reboot after installation)
- Added "Delete" to the context menu in the main window
- Fixed file size of black and white images after rotate/crop
- Fixed cancel in OCR download progress window
- Fixed issues with the default profile logic
- Fixed various translation-related issues

Changes in 4.3.1:
- Bug fixes

Changes in 4.3.0:
- New feature: Batch scan (under the Scan menu)
- New feature: Bulk image editing (brightness/contrast/crop/custom rotation)
- Added "Alternative Interleave" function for interleaving duplexed pages in a different order
- Added Finnish language
- Bug fixes

Changes in 4.2.3:
- Added Greek and Estonian languages
- Added support for multiple OCR languages on the command line (e.g. "--ocrlang eng+fra")
- Fixed an issue with importing certain PDFs
- Fixed an issue that caused a black background when rotating and saving as certain formats
- Fixed an issue that caused duplicate close prompts
- Improved responsiveness while importing large PDFs

Changes in 4.2.2:
- Fixed an issue with OCR for non-English languages
- Fixed an issue with missing translations for Move Up/Down buttons

Changes in 4.2.1:
- Fixed an issue where focus is lost when scanning from the Profiles window
- Fixed an issue where Native WIA doesn't work properly

Changes in 4.2.0:
- Added a "Delete" button to the preview window
- Added new keyboard shortcuts to the preview window: Esc (close), Page Up (prev), Page Down (next)
- Added unicode support to PDF metadata and OCR text
- Bug fixes

Changes in 4.1.1:
- New language: Romanian
- New language: Norwegian (Bokmål)
- Bug fixes

Changes in 4.1.0:
- Changed the website link in the About window to www.naps2.com
- Changed "Substitutions" to "Placeholders" for consistency with other software
- Bug fixes

Changes in 4.0b3:
- New feature: Thumbnails can be resized for easier viewing
- New feature: Substitutions can be used in both the GUI and NAPS2.Console when saving (e.g. "$(YYYY)-$(MM)-$(DD) $(nn).pdf" to include the date and an incrementing number)
- New feature: Image settings (default file name, jpeg quality), and default file name setting in PDF settings
- Bug fixes

Changes in 4.0b2:
- New feature: PDF settings (metadata, encryption) and email settings (can change attachment name)
- Changed format of standalone/portable archives for easier usage
- Scanning multiple pages with WIA no longer steals focus from other applications
- Scanning with WIA in NAPS2.Console no longer displays a separate window
- Bug fixes

Changes in 4.0b1:
- Merged the Quick Scan functionality into the toolbar
- Merged the previous Scan functionality into the Profiles window
- New feature: Image Editing - Crop, Brightness, Contrast, Custom Rotation
- New feature: Enhanced Preview Window - Can now browse through the images one-by-one and also edit them
- New feature: Print scanned images directly from NAPS2
- New feature: Prompt when trying to exit with unsaved changes
- New feature: The file type used when saving images is remembered
- Added more keyboard shortcuts (Ctrl+S for save all as PDF, Ctrl+O for import, Ctrl+Enter for scan)

Changes in 3.3.5:
- Bug fix: Added missing OCR languages

Changes in 3.3.4:
- New language: Turkish
- Bug fix: Searching PDFs generated with OCR should now work for all readers
- Bug fix: Fixed issue with some TWAIN devices when scanning fails

Changes in 3.3.3:
- Minor bug fixes

Changes in 3.3.2:
- Bug fixes

Changes in 3.3.1:
- Bug fix: Fixed issue with TWAIN

Changes in 3.3.0:
- New feature: TWAIN with predefined settings
- New feature: OCR options in command-line interface
- New language: Chinese (Taiwan)

Changes in 3.2.1:
- New language: Albanian
- Bug fix: Increase time allotted for OCR

Changes in 3.2.0:
- New feature: Custom page sizes
- Added built-in B5 and B4 page size options
- Added 400 and 800 dpi options

Changes in 3.1.1:
- New language: Swedish
- Bug fix: Dutch language added to installer

Changes in 3.1.0:
- New feature: One-click scan
- New feature: Can reverse the order of all or some pages with a single click
- New languages: Croatian, Dutch
- Bug fix: Prevent downloading corrupted OCR files
- Bug fix: Resolve some issues when scanning from document feeders

Changes in 3.0b1:
- New feature: OCR (Optical Character Recognition) to make PDF files searchable
- New feature: Can import PDF and image files (e.g. to resume a previous scanning session)
- New feature: Option to save selected pages only
- New feature: Can re-order (interleave) pages with a single click
- New feature: Added a right-click menu and the ability to copy images to the clipboard
- New feature: Added a 150dpi option to WIA settings
- Bug fix: Incorrect page size for black and white images
- Bug fix: Duplex scanning (for some models)
- Various other changes and bug fixes

Changes in 2.6.3:
- Added Bulgarian translation
- Added Portuguese translations

Changes in 2.6.2:
- Added Danish translation

Changes in 2.6.1:
- Fixed a bug when scanning after clearing previously scanned images
- Fixed an error in NAPS2.Console's help text

Changes in 2.6:
- Added Czech, French, and Polish translations
- Fixed Catalan translation when using EXE installer

Changes in 2.5:
- Command-line interface (naps2.console.exe) can send emails
- More windows can be resized, and all windows remember their size and position
- NAPS2 will offer to recover scanned images if it previously closed unexpectedly
- Substantially reduced memory usage
- Added Hebrew and Catalan translations
- Bug fixes

Changes in 2.4:
- Profiles can now be created without specifying a device (the device will be chosen when scanning)
- Organizations can now configure some application settings in appsettings.xml (see Wiki)
- Updated German translation
- Bug fixes

Changes in 2.3:
- Added German and Italian translations

Changes in 2.2:
- Added Russian translation
- Updated Ukrainian translation
- Various bug fixes

Changes in 2.1:
- Added language dropdown
- Added translations for Spanish and Ukrainian

Changes in 2.0:
- Major bug fixes for TWAIN on x64 and native WIA
- Added command-line interface (naps2.console.exe)
- Added logging capabilities for error reporting
- Changed .NET dependency from 3.5 Client Profile to 4.0 Client Profile

Changes in 1.0b2:
- Added Clear button to toolbar
- Added Ctrl+A shortcut to select all thumbnails
- The last-used profile is now remembered and used as the default
- Fix for crash when scanning with the "Black and White" option (credit to Peter De Leeuw)
- Fix for crash when trying to use an offline scanner (WIA)

Changes in 1.0b1:
- Now requires .NET framework 3.5 (or later)
- New icons
- Better user experience
- Admin no longer required to save profiles
- Various other bug fixes and minor enhancements