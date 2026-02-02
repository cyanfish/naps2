# PDF Signature Field Feature - Implementation Summary

## Overview
This document summarizes the implementation of the PDF signature field placement feature for NAPS2.

## Feature Description
Users can now place signature fields on PDF pages using a mouse-drag interface. The signature fields are persisted in exported PDFs using pyHanko (vendored Python library).

## Files Added

### 1. Data Model
- **`NAPS2.Sdk/Pdf/SignatureFieldPlacement.cs`**
  - Record type for storing signature field placements
  - Uses normalized coordinates (0.0-1.0) for resolution independence
  - Provides conversion methods between normalized and pixel coordinates

### 2. UI Components
- **`NAPS2.Lib/EtoForms/Ui/SignatureFieldForm.cs`**
  - Modal form for placing signature fields via mouse drag
  - Based on existing `CropForm` pattern
  - Shows existing fields and allows placing new ones
  - Stores fields in image's `PostProcessingData`

### 3. PDF Export Integration
- **`NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs`**
  - C# helper class to invoke Python script
  - Finds Python executable and script path
  - Handles graceful degradation when Python/pyHanko unavailable
  - Converts normalized coordinates to PDF points

### 4. Python Script
- **`scripts/embed_signature_fields.py`**
  - Python script using vendored pyHanko to embed signature fields
  - Automatically adds vendored pyHanko source to Python path
  - Takes input PDF, output PDF, and JSON field data
  - Handles coordinate conversion (PDF uses bottom-left origin)
  - Provides clear error messages for missing dependencies

### 5. Vendored Dependencies
- **`third_party/pyHanko/`** (Git submodule)
  - pyHanko source code vendored as a git submodule
  - Pinned to commit: b89f139e5c5e0f9895a39686ff2dc4c74dd23ba8
  - Includes pyHanko and pyhanko-certvalidator packages
  - Licensed under MIT (see `third_party/pyHanko/LICENSE`)

- **`scripts/requirements-signature-fields.txt`**
  - Lists runtime dependencies required by pyHanko
  - Users only need to install these dependencies, not pyHanko itself

### 6. Documentation
- **`docs/SIGNATURE_FIELD_TESTING.md`**
  - Comprehensive manual testing procedure
  - Prerequisites and setup instructions
  - Expected results and troubleshooting guide

## Files Modified

### 1. Data Model Extension
- **`NAPS2.Sdk/Images/PostProcessingData.cs`**
  - Added `SignatureFields` property (nullable list)
  - Allows storing signature field placements with each image

### 2. PDF Export Pipeline
- **`NAPS2.Sdk/Pdf/PdfExporter.cs`**
  - Added `EmbedSignatureFields()` method
  - Collects signature fields from all pages after PDF creation
  - Invokes `SignatureFieldEmbedder` to apply fields
  - Handles both file and stream outputs

### 3. UI Integration
- **`NAPS2.Lib/EtoForms/Desktop/IDesktopSubFormController.cs`**
  - Added `ShowSignatureFieldForm()` method declaration

- **`NAPS2.Lib/EtoForms/Desktop/DesktopSubFormController.cs`**
  - Implemented `ShowSignatureFieldForm()` method

- **`NAPS2.Lib/EtoForms/Ui/DesktopCommands.cs`**
  - Added `SignatureField` command property
  - Initialized command with text and icon

- **`NAPS2.Lib/EtoForms/Ui/DesktopForm.cs`**
  - Added `SignatureField` command to Image menu
  - Placed after Crop command for logical grouping

## Architecture Decisions

### 1. Coordinate System
- **Normalized Coordinates**: Fields stored as fractions (0.0-1.0) of page dimensions
- **Rationale**: Resolution-independent, works across different page sizes and DPI settings
- **Conversion**: Happens at export time based on actual PDF page dimensions

### 2. Storage Location
- **PostProcessingData**: Signature fields stored alongside other metadata (barcode, page number, etc.)
- **Rationale**: Consistent with existing architecture, properly disposed with image lifecycle
- **Limitation**: Fields are session-only (not persisted when reopening NAPS2)

### 3. Python Integration
- **Subprocess Invocation**: C# spawns Python process to run pyHanko script
- **Vendored Source**: pyHanko source is included in the repository as a git submodule
- **Rationale**: pyHanko is a mature Python library; reimplementing in C# would be complex
- **Graceful Degradation**: If Python/dependencies unavailable, PDF exports without fields (with warning)

### 4. UI Pattern
- **Modal Form**: Based on existing `UnaryImageFormBase` pattern (like CropForm)
- **Mouse Drag**: Familiar interaction pattern for defining rectangular areas
- **Visual Feedback**: Semi-transparent overlay shows field placement

## Dependencies

### Required for Full Functionality
- **Python 3.x**: Must be in system PATH
- **pyHanko runtime dependencies**: Install via `pip install -r scripts/requirements-signature-fields.txt`
  - asn1crypto, tzlocal, requests, pyyaml, cryptography, lxml, oscrypto, uritools
- **pyHanko source**: Vendored in `third_party/pyHanko` (no separate installation needed)

### Fallback Behavior
- If Python not found: Warning logged, PDF exports without signature fields
- If dependencies not installed: Warning logged, PDF exports without signature fields
- If script not found: Warning logged, PDF exports without signature fields

## Known Limitations

1. **Session-Only Storage**: Signature fields not persisted when closing/reopening NAPS2
2. **No Field Editing**: Cannot edit or delete placed fields (must reopen tool to add more)
3. **Auto-Generated Names**: Field names are GUIDs, not user-customizable
4. **Single Field Per Session**: MVP allows placing one field at a time per page
5. **No Visual Indicator**: Thumbnails don't show which pages have signature fields

## Testing

### Manual Testing
Follow the procedure in [`docs/SIGNATURE_FIELD_TESTING.md`](../docs/SIGNATURE_FIELD_TESTING.md)

### Key Test Cases
1. Place signature field and export to PDF
2. Verify field appears in PDF viewer (Adobe Acrobat, Foxit, etc.)
3. Test with Python/dependencies unavailable (graceful degradation)
4. Test multiple fields on same page
5. Test fields on multiple pages

## Future Enhancements

Potential improvements for future versions:
- Persistent storage of signature fields (save/load with project)
- Field editing/deletion UI
- Custom field names and properties
- Visual indicators in thumbnail view
- Support for other field types (text, checkbox, radio button)
- Batch placement across multiple pages
- Field templates/presets

## Assumptions

As specified in the task:
1. **Coordinate Mapping**: Normalized fractions ensure accuracy across typical page sizes
2. **pyHanko Suitability**: Confirmed as appropriate for AcroForm signature field embedding
3. **Conservative Approach**: Minimal changes to existing code, feature behind UI command
4. **Error Handling**: Robust error messages when dependencies unavailable

## Build Considerations

- No breaking changes to existing PDF export when feature unused
- All new files follow existing NAPS2 code conventions
- Python script is standalone and can be tested independently
- C# code compiles on all supported platforms (Windows, macOS, Linux)

## Summary

This implementation provides a complete MVP for PDF signature field placement:
- ✅ UI tool for mouse-drag field placement
- ✅ Data model with normalized coordinates
- ✅ PDF export integration with pyHanko
- ✅ Graceful degradation when dependencies unavailable
- ✅ Comprehensive testing documentation
- ✅ No breaking changes to existing functionality
