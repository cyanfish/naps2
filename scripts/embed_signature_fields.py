#!/usr/bin/env python3
"""
NAPS2 Signature Field Embedder
Uses pyHanko to embed signature fields into PDF documents.

Usage:
    python embed_signature_fields.py <input_pdf> <output_pdf> <fields_json>

Where fields_json is a JSON array of signature field objects with:
    - name: field name
    - page: page number (0-indexed)
    - x: x coordinate in PDF points (72 points = 1 inch)
    - y: y coordinate in PDF points (from bottom-left)
    - width: field width in PDF points
    - height: field height in PDF points
"""

import sys
import json
import os
from pathlib import Path

# Add vendored pyHanko to Python path
# This allows importing pyHanko from the repository without requiring pip install
script_dir = Path(__file__).parent
repo_root = script_dir.parent
pyhanko_src = repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko" / "src"
certvalidator_src = repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko-certvalidator" / "src"

# Insert vendored paths at the beginning to prioritize them over system installations
if pyhanko_src.exists():
    sys.path.insert(0, str(pyhanko_src))
if certvalidator_src.exists():
    sys.path.insert(0, str(certvalidator_src))

def check_dependencies():
    """Check if pyHanko is installed."""
    try:
        import pyhanko
        from pyhanko.pdf_utils import generic
        from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
        from pyhanko.sign.fields import SigFieldSpec
        return True
    except ImportError:
        return False

def embed_signature_fields(input_pdf, output_pdf, fields_data):
    """
    Embed signature fields into a PDF using pyHanko.
    
    Args:
        input_pdf: Path to input PDF file
        output_pdf: Path to output PDF file
        fields_data: List of field dictionaries
    """
    from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
    from pyhanko.sign.fields import SigFieldSpec, append_signature_field
    from pyhanko.pdf_utils import generic
    from pyhanko.pdf_utils.content import RawContent
    from pyhanko.pdf_utils.layout import BoxConstraints
    from pyhanko.pdf_utils.generic import pdf_name
    
    with open(input_pdf, 'rb') as inf:
        writer = IncrementalPdfFileWriter(inf)
        
        for field in fields_data:
            # Create signature field specification with empty appearance
            # pyHanko uses bottom-left origin, coordinates in PDF points
            width = field['width']
            height = field['height']
            
            spec = SigFieldSpec(
                sig_field_name=field['name'],
                on_page=field['page'],
                box=(
                    field['x'],
                    field['y'],
                    field['x'] + width,
                    field['y'] + height
                ),
                empty_field_appearance=False  # We'll create our own appearance
            )
            
            # Add the field to the PDF
            append_signature_field(writer, spec)
            
            # Now find the field we just added and enhance its appearance
            # Get the field reference from the AcroForm
            fields_array = writer.root['/AcroForm']['/Fields']
            sig_field_ref = fields_array[-1]  # The field we just added
            sig_field = sig_field_ref.get_object()
            
            # Create a custom appearance stream with text and border
            appearance_cmds = [
                b'q',  # Save graphics state
                # Draw light gray background
                b'0.95 0.95 0.95 rg',
                b'0 0 %g %g re f' % (width, height),
                # Draw border
                b'0.5 w',  # Line width
                b'0.3 0.3 0.3 RG',  # Dark gray border
                b'0 0 %g %g re S' % (width, height),
                # Add text "Sign here"
                b'BT',  # Begin text
                b'/Helvetica 12 Tf',  # Font and size
                b'0.3 0.3 0.3 rg',  # Text color (dark gray)
                b'%g %g Td' % (10, height / 2 - 6),  # Position text
                b'(Sign here) Tj',  # Draw text
                b'ET',  # End text
                b'Q',  # Restore graphics state
            ]
            
            # Create the appearance stream
            # Build the form XObject manually since RawContent expects a specific resource format
            ap_stream = generic.StreamObject(stream_data=b' '.join(appearance_cmds))
            ap_stream[pdf_name('/Type')] = pdf_name('/XObject')
            ap_stream[pdf_name('/Subtype')] = pdf_name('/Form')
            ap_stream[pdf_name('/BBox')] = generic.ArrayObject([
                generic.NumberObject(0),
                generic.NumberObject(0),
                generic.NumberObject(width),
                generic.NumberObject(height)
            ])
            ap_stream[pdf_name('/Resources')] = generic.DictionaryObject({
                pdf_name('/Font'): generic.DictionaryObject({
                    pdf_name('/Helvetica'): generic.DictionaryObject({
                        pdf_name('/Type'): pdf_name('/Font'),
                        pdf_name('/Subtype'): pdf_name('/Type1'),
                        pdf_name('/BaseFont'): pdf_name('/Helvetica'),
                    })
                })
            })
            
            # Update the appearance dictionary
            if pdf_name('/AP') not in sig_field:
                sig_field[pdf_name('/AP')] = generic.DictionaryObject()
            sig_field[pdf_name('/AP')][pdf_name('/N')] = writer.add_object(ap_stream)
            writer.update_container(sig_field)
        
        # Write the modified PDF
        with open(output_pdf, 'wb') as outf:
            writer.write(outf)

def main():
    """Main entry point."""
    if len(sys.argv) != 4:
        print("Usage: python embed_signature_fields.py <input_pdf> <output_pdf> <fields_json>", file=sys.stderr)
        sys.exit(1)
    
    # Check dependencies
    if not check_dependencies():
        print("ERROR: pyHanko dependencies are not available.", file=sys.stderr)
        print("Install dependencies with: pip install -r scripts/requirements-signature-fields.txt", file=sys.stderr)
        sys.exit(2)
    
    input_pdf = sys.argv[1]
    output_pdf = sys.argv[2]
    fields_json = sys.argv[3]
    
    # Validate input file exists
    if not os.path.exists(input_pdf):
        print(f"ERROR: Input PDF not found: {input_pdf}", file=sys.stderr)
        sys.exit(3)
    
    # Parse fields data
    try:
        fields_data = json.loads(fields_json)
    except json.JSONDecodeError as e:
        print(f"ERROR: Invalid JSON: {e}", file=sys.stderr)
        sys.exit(4)
    
    # Embed signature fields
    try:
        embed_signature_fields(input_pdf, output_pdf, fields_data)
        print(f"SUCCESS: Signature fields embedded in {output_pdf}")
    except Exception as e:
        print(f"ERROR: Failed to embed signature fields: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc(file=sys.stderr)
        sys.exit(5)

if __name__ == '__main__':
    main()
