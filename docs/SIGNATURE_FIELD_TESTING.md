# PDF Signature Field Placement - Manual Test Procedure

## Overview
This document describes how to manually test the PDF signature field placement feature in NAPS2.

## Prerequisites

### Required Software
1. **NAPS2** - Build and run the application
2. **Python 3.x** - Required for signature field embedding
3. **pyHanko dependencies** - Python libraries required by pyHanko (vendored in this repository)
   ```bash
   pip install -r scripts/requirements-signature-fields.txt
   ```
   Note: pyHanko itself is vendored in `third_party/pyHanko` and does not need to be installed separately.
4. **PDF Viewer** - Adobe Acrobat Reader, Foxit Reader, or similar that displays form fields

### Optional (for verification)
- A PDF viewer that highlights form fields (e.g., Adobe Acrobat Reader)

## Test Procedure

### Part 1: Setup and Basic Functionality

1. **Start NAPS2**
   - Launch the NAPS2 application
   - Ensure you have at least one scanned or imported image/PDF page

2. **Access the Signature Field Tool**
   - Select an image/page in the thumbnail list
   - Go to the **Image** menu
   - Click on **Place Signature Field** (or use the toolbar button if visible)
   - The signature field placement dialog should open

3. **Place a Signature Field**
   - In the signature field dialog, you should see your selected page displayed
   - Click and drag on the page to create a rectangle
   - The rectangle should appear with a colored border while dragging
   - Release the mouse button to finalize the field placement
   - The field should remain visible as a semi-transparent overlay

4. **Apply the Field**
   - Click the **OK** button to apply the signature field
   - The dialog should close
   - The field placement is now stored with the image

### Part 2: Export and Verification

5. **Export to PDF**
   - With the image(s) that have signature fields, go to **File** → **Save PDF** (or **Save All as PDF**)
   - Choose a location and filename for the PDF
   - Click **Save**
   - Wait for the export to complete

6. **Check Export Logs** (Optional)
   - If Python or pyHanko dependencies are not installed, you should see a warning in the logs
   - The PDF will still be created, but without embedded signature fields
   - If dependencies are installed, you should see a success message

7. **Verify Signature Fields in PDF**
   - Open the exported PDF in a PDF viewer that supports form fields
   - **Adobe Acrobat Reader**: 
     - The signature field should appear as a clickable area
     - Right-click on the field → **Properties** to see field details
   - **Foxit Reader**:
     - Signature fields should be visible and highlighted
   - **Preview (macOS)**:
     - May not show signature fields (limited form support)

### Part 3: Multiple Fields and Pages

8. **Place Multiple Fields**
   - Open the signature field tool again on the same page
   - Place another signature field in a different location
   - Click **OK**
   - Both fields should be stored

9. **Fields on Different Pages**
   - If you have multiple pages, select a different page
   - Open the signature field tool
   - Place a signature field on this page
   - Export to PDF again

10. **Verify Multiple Fields**
    - Open the exported PDF
    - Navigate through pages
    - Verify that signature fields appear on the correct pages
    - Each field should be independently clickable

### Part 4: Edge Cases

11. **No Python/pyHanko Dependencies Installed**
    - Temporarily rename or remove Python from PATH
    - Place signature fields and export
    - Verify that:
      - Export completes without crashing
      - A warning is logged
      - PDF is created (without signature fields)

12. **Empty Field Placement**
    - Open the signature field tool
    - Click **OK** without placing any field
    - Verify no error occurs

13. **Very Small Fields**
    - Place a very small signature field (just a few pixels)
    - Export and verify it appears in the PDF

14. **Very Large Fields**
    - Place a signature field covering most of the page
    - Export and verify it appears correctly

## Expected Results

### Success Criteria
✅ Signature field tool opens without errors  
✅ User can drag to create a visible rectangle  
✅ Field placement is stored with the image  
✅ PDF export completes successfully  
✅ When pyHanko dependencies are available, signature fields are embedded in PDF  
✅ Signature fields are visible and functional in PDF viewers  
✅ Multiple fields can be placed on the same page  
✅ Fields can be placed on different pages  
✅ Graceful degradation when Python/pyHanko dependencies are unavailable  

### Known Limitations
- Signature fields are not editable after placement (must reopen tool to add more)
- Field names are auto-generated (not user-customizable in MVP)
- No visual indicator in thumbnail view showing which pages have signature fields
- Signature fields are not preserved when reopening NAPS2 (stored in session only)

## Troubleshooting

### Issue: Signature field tool doesn't appear in menu
- **Solution**: Ensure you have selected at least one image/page first

### Issue: Python not found error
- **Solution**: Install Python 3.x and ensure it's in your system PATH
- **Verify**: Run `python --version` or `python3 --version` in terminal

### Issue: pyHanko dependencies not installed error
- **Solution**: Run `pip install -r scripts/requirements-signature-fields.txt` in terminal
- **Verify**: Run `python -c "import pyhanko; print('OK')"` (should work with vendored pyHanko)

### Issue: Signature fields don't appear in PDF
- **Solution**: 
  1. Check that Python and pyHanko dependencies are installed
  2. Check application logs for errors
  3. Try a different PDF viewer (some viewers don't display form fields)

### Issue: Script not found error
- **Solution**: Ensure `scripts/embed_signature_fields.py` exists in the repository
- The script should be found relative to the application directory

## Test Report Template

```
Test Date: _______________
Tester: _______________
NAPS2 Version: _______________
Python Version: _______________
pyHanko Dependencies Installed: Yes / No

Test Results:
[ ] Part 1: Setup and Basic Functionality - PASS / FAIL
[ ] Part 2: Export and Verification - PASS / FAIL  
[ ] Part 3: Multiple Fields and Pages - PASS / FAIL
[ ] Part 4: Edge Cases - PASS / FAIL

Notes:
_________________________________
_________________________________
_________________________________
```

## Additional Notes

- The signature field feature is designed as an MVP (Minimum Viable Product)
- Future enhancements may include:
  - Persistent storage of signature fields
  - Field editing/deletion UI
  - Custom field names and properties
  - Visual indicators in thumbnail view
  - Support for other field types (text, checkbox, etc.)
