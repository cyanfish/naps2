#!/usr/bin/env python3
"""scripts/build_embedder_helper.py

Build script for creating a self-contained executable of the signature field embedder.

This script uses Nuitka to compile [`scripts/embed_signature_fields.py`](scripts/embed_signature_fields.py:1)
into a single-file executable that bundles all runtime dependencies (including the vendored pyHanko source).

Supported platforms:
  - macOS  (outputs to build/macos/)
  - Linux  (outputs to build/linux/)
  - Windows (outputs to build/windows/)

Prerequisites:
  - Python 3.11+
  - Nuitka installed (recommended: `pip install -e ".[build]"`)
  - Platform build toolchain:
      macOS: Xcode Command Line Tools
      Linux: GCC/Clang toolchain
      Windows: MSVC Build Tools

Output:
  - build/<platform>/naps2-signature-helper[.exe]
"""

import argparse
import os
import subprocess
import sys
from pathlib import Path
from typing import NoReturn


def error_exit(message: str, code: int = 1) -> NoReturn:
    """Print error message and exit with specified code."""
    print(f"ERROR: {message}", file=sys.stderr)
    sys.exit(code)


def check_prerequisites() -> None:
    """Validate that all prerequisites are met before building."""
    # Check if Nuitka is available
    try:
        import nuitka  # noqa: F401
    except ImportError:
        error_exit(
            "Nuitka is not installed. Install with: uv pip install -e '.[build]'",
            code=2,
        )

    # Check if pyHanko submodule is present
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    pyhanko_src = repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko" / "src"
    certvalidator_src = (
        repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko-certvalidator" / "src"
    )

    if not pyhanko_src.exists():
        error_exit(
            f"pyHanko source not found at {pyhanko_src}. "
            "Initialize submodules with: git submodule update --init --recursive",
            code=3,
        )

    if not certvalidator_src.exists():
        error_exit(
            f"pyhanko-certvalidator source not found at {certvalidator_src}. "
            "Initialize submodules with: git submodule update --init --recursive",
            code=3,
        )

    # Check if source script exists
    source_script = script_dir / "embed_signature_fields.py"
    if not source_script.exists():
        error_exit(f"Source script not found: {source_script}", code=4)

    print("✓ All prerequisites met")


def detect_platform() -> str:
    if sys.platform.startswith("darwin"):
        return "macos"
    if sys.platform.startswith("win"):
        return "windows"
    return "linux"


def build_executable(platform: str, macos_target_arch: str | None, output_dir: Path | None) -> None:
    """Build the standalone executable using Nuitka."""
    script_dir = Path(__file__).parent
    repo_root = script_dir.parent
    source_script = script_dir / "embed_signature_fields.py"
    resolved_output_dir = output_dir or (repo_root / "build" / platform)

    # Create output directory if it doesn't exist
    resolved_output_dir.mkdir(parents=True, exist_ok=True)

    # Paths to vendored dependencies
    pyhanko_src = repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko" / "src"
    certvalidator_src = (
        repo_root / "third_party" / "pyHanko" / "pkgs" / "pyhanko-certvalidator" / "src"
    )

    print(f"Building executable from: {source_script}")
    print(f"Platform: {platform}")
    print(f"Output directory: {resolved_output_dir}")
    print(f"Including vendored pyHanko from: {pyhanko_src}")
    print(f"Including vendored certvalidator from: {certvalidator_src}")

    # Nuitka command-line arguments
    # Using subprocess instead of Python API for better control and error reporting
    nuitka_args: list[str] = [
        sys.executable,
        "-m",
        "nuitka",
        # Basic compilation options
        "--standalone",  # Create standalone distribution with all dependencies
        "--onefile",  # Create a single executable file
        # Output configuration
        f"--output-dir={resolved_output_dir}",
        "--output-filename=naps2-signature-helper",
        # Optimization options
        "--assume-yes-for-downloads",  # Auto-accept dependency downloads
        "--remove-output",  # Remove build directory after successful build
        # Python path configuration - include vendored dependencies
        f"--include-package-data=pyhanko",
        f"--include-package-data=pyhanko_certvalidator",
        # Follow all imports to ensure complete bundling
        "--follow-imports",
        # Include standard library modules that might be needed
        "--include-module=json",
        "--include-module=pathlib",
        "--include-module=sys",
        "--include-module=os",
        # Include all pyHanko subpackages
        "--include-package=pyhanko.pdf_utils",
        "--include-package=pyhanko.sign",
        "--include-package=pyhanko_certvalidator",
        # Include dependencies
        "--include-package=asn1crypto",
        "--include-package=cryptography",
        "--include-package=lxml",
        "--include-package=oscrypto",
        "--include-package=requests",
        "--include-package=tzlocal",
        "--include-package=uritools",
        # Exclude unnecessary modules
        "--nofollow-import-to=tkinter",
        "--nofollow-import-to=unittest",
        "--nofollow-import-to=test",
        # Disable console window (optional, can be removed if debugging needed)
        "--disable-console",
        # Show progress
        "--show-progress",
        "--show-modules",
        # Source file
        str(source_script),
    ]

    # Platform-specific switches
    if platform == "macos":
        arch = macos_target_arch or "arm64"
        # Insert after module invocation (sys.executable -m nuitka)
        # Final argv starts with: python -m nuitka --macos-target-arch=... <other args>
        nuitka_args[3:3] = [f"--macos-target-arch={arch}"]

    print("\nStarting Nuitka compilation...")
    print("This may take several minutes...\n")

    try:
        # Set PYTHONPATH to include vendored dependencies
        env = os.environ.copy()
        pythonpath_parts = [str(pyhanko_src), str(certvalidator_src)]
        if "PYTHONPATH" in env:
            pythonpath_parts.append(env["PYTHONPATH"])
        env["PYTHONPATH"] = os.pathsep.join(pythonpath_parts)

        # Run Nuitka
        result = subprocess.run(
            nuitka_args,
            env=env,
            check=True,
            capture_output=False,  # Show output in real-time
        )

        if result.returncode == 0:
            print("\n✓ Build completed successfully!")
            exe_name = "naps2-signature-helper.exe" if platform == "windows" else "naps2-signature-helper"
            print(f"Executable location: {resolved_output_dir / exe_name}")
            exe_name = "naps2-signature-helper.exe" if platform == "windows" else "naps2-signature-helper"
            print("\nUsage:")
            print(f"  {resolved_output_dir / exe_name} <input_pdf> <output_pdf> <fields_json>")
        else:
            error_exit(f"Nuitka compilation failed with code {result.returncode}", code=5)

    except subprocess.CalledProcessError as e:
        error_exit(f"Nuitka compilation failed: {e}", code=5)
    except FileNotFoundError:
        error_exit(
            "Python executable not found. Ensure you're running in a proper Python environment.",
            code=6,
        )
    except Exception as e:
        error_exit(f"Unexpected error during build: {e}", code=7)


def main() -> None:
    """Main entry point for the build script."""
    print("=" * 70)
    print("NAPS2 Signature Helper - Build Script")
    print("=" * 70)
    print()

    parser = argparse.ArgumentParser(add_help=True)
    parser.add_argument(
        "--platform",
        choices=["macos", "linux", "windows", "auto"],
        default="auto",
        help="Target platform (default: auto-detect)",
    )
    parser.add_argument(
        "--macos-target-arch",
        default="arm64",
        help="macOS target architecture for Nuitka (default: arm64)",
    )
    parser.add_argument(
        "--output-dir",
        default=None,
        help="Override output directory (default: build/<platform>/)",
    )
    args = parser.parse_args()

    platform = detect_platform() if args.platform == "auto" else args.platform
    out_dir = Path(args.output_dir).expanduser().resolve() if args.output_dir else None

    # Check prerequisites
    print("Checking prerequisites...")
    check_prerequisites()
    print()

    # Build executable
    build_executable(platform=platform, macos_target_arch=args.macos_target_arch, output_dir=out_dir)


if __name__ == "__main__":
    main()
