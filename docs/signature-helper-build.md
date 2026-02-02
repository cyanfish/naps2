# Building & Updating the macOS Signature Helper

This document explains how to build and update the **signature helper** executable shipped with the macOS build of NAPS2.

Primary entry points:

* Build wrapper: [`scripts/build_helper.sh`](../scripts/build_helper.sh:1)
* Build script: [`scripts/build_embedder_helper.py`](../scripts/build_embedder_helper.py:1)
* Source (Python): [`scripts/embed_signature_fields.py`](../scripts/embed_signature_fields.py:1)
* Build dependencies: [`pyproject.toml`](../pyproject.toml:1) (`[project.optional-dependencies]` → `build`)
* macOS packaging hook: [`MacPackager.IncludeSignatureHelper()`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:92)

---

## 1) Overview

### What is the signature helper?

The signature helper is a small macOS executable that embeds **AcroForm signature fields** into a PDF.

Functionally, it is a compiled form of the Python script [`scripts/embed_signature_fields.py`](../scripts/embed_signature_fields.py:1), which uses the vendored pyHanko source tree to modify PDFs.

At runtime, NAPS2 prefers to call the helper (no Python interpreter required). If the helper cannot be found, NAPS2 falls back to running the Python script.

Runtime integration is implemented primarily in [`SignatureFieldEmbedder.EmbedFields()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:28), specifically:

* Helper discovery: [`SignatureFieldEmbedder.FindBundledHelper()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:242)
* Fallback script discovery: [`SignatureFieldEmbedder.FindScriptPath()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:356)
* Fallback Python discovery: [`SignatureFieldEmbedder.FindPythonExecutable()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:301)

### Why bundle an executable instead of running Python?

Bundling a helper executable has several benefits over invoking a Python script:

* **No Python runtime required** on the end user’s machine.
* **Predictable dependency set**: the build step bundles the runtime dependencies needed by pyHanko.
* **Better UX**: fewer “Python not found” / “dependency missing” failures.
* **App-store/notarization friendly**: packaging can include the helper and code signing can cover it as part of the bundle.

The fallback to Python remains useful for development and for environments where the helper is not present.

---

## 2) Prerequisites

### Required tooling

1. **Python 3.11+**
   * The repository requires `>=3.11` (see [`pyproject.toml`](../pyproject.toml:6)).

2. **uv** (Astral’s Python package manager)
   * The wrapper script [`scripts/build_helper.sh`](../scripts/build_helper.sh:1) uses `uv` for venv creation and dependency installation.

3. **Nuitka**
   * The build script checks for Nuitka at startup (see [`check_prerequisites()`](../scripts/build_embedder_helper.py:30)).
   * Nuitka is provided via the `build` optional dependency group (see [`pyproject.toml`](../pyproject.toml:18)).

4. **Xcode Command Line Tools**
   * Required for compiling native extensions and linking the final executable.
   * Install:
     ```bash
     xcode-select --install
     ```

### macOS requirements

* The helper build is macOS-specific.
* The current build configuration targets Apple Silicon (`arm64`) via [`--macos-target-arch=arm64`](../scripts/build_embedder_helper.py:106).

### Git submodules (pyHanko)

The helper bundles vendored pyHanko and pyhanko-certvalidator from the pyHanko git submodule.

* Submodule documentation: [`third_party/README.md`](../third_party/README.md:1)
* The build script verifies the submodule by checking for the vendored source directories (see [`check_prerequisites()`](../scripts/build_embedder_helper.py:30)):
  * [`third_party/pyHanko/pkgs/pyhanko/src`](../third_party/pyHanko/pkgs/pyhanko/src)
  * [`third_party/pyHanko/pkgs/pyhanko-certvalidator/src`](../third_party/pyHanko/pkgs/pyhanko-certvalidator/src)

Initialize submodules:

```bash
git submodule update --init --recursive
```

---

## 3) Building the Helper

### Recommended: wrapper script

Use the wrapper [`scripts/build_helper.sh`](../scripts/build_helper.sh:1). It:

* checks that `uv` is installed,
* creates a `.venv` if missing,
* installs build dependencies from [`pyproject.toml`](../pyproject.toml:1),
* runs the build script with `uv run`.

From the repository root:

```bash
./scripts/build_helper.sh
```

### Direct: invoke the Python build script

If you want to run the build without the wrapper, ensure you have a venv and build deps installed:

```bash
uv venv
uv pip install -e ".[build]"
uv run python ./scripts/build_embedder_helper.py
```

### Expected output

The build script writes into the output directory configured in [`build_executable()`](../scripts/build_embedder_helper.py:71) and uses the filename set by [`--output-filename=naps2-signature-helper`](../scripts/build_embedder_helper.py:103).

On success, the script prints:

* “Build completed successfully!”
* “Executable location: …” (printed by [`build_executable()`](../scripts/build_embedder_helper.py:71))

### Quick verification

1. Confirm the file exists and is executable:

   ```bash
   ls -la ./build/macos/
   file ./build/macos/naps2-signature-helper
   ```

2. Run the helper on any PDF you have available:

   ```bash
   ./build/macos/naps2-signature-helper \
     ./input.pdf \
     ./output.pdf \
     '[{"name":"Sig1","page":0,"x":72,"y":72,"width":200,"height":50}]'
   ```

3. Open `output.pdf` in a PDF viewer with form support (Adobe Acrobat Reader, Foxit, etc.) to confirm the signature widget is present.

### Troubleshooting

#### Build fails: “uv is not installed”

* This comes from [`scripts/build_helper.sh`](../scripts/build_helper.sh:38).
* Install uv (see https://github.com/astral-sh/uv) and retry.

#### Build fails: “Nuitka is not installed”

* This comes from [`check_prerequisites()`](../scripts/build_embedder_helper.py:30).
* Fix by installing the build dependency group:

  ```bash
  uv pip install -e ".[build]"
  ```

#### Build fails: “pyHanko source not found … Initialize submodules …”

* This comes from [`check_prerequisites()`](../scripts/build_embedder_helper.py:41).
* Fix by initializing submodules:

  ```bash
  git submodule update --init --recursive
  ```

#### Build fails on compilation/linking

Common causes:

* Missing Xcode Command Line Tools
  ```bash
  xcode-select --install
  ```
* A stale or partially-built Nuitka output directory
  * The build uses [`--remove-output`](../scripts/build_embedder_helper.py:109), but if a previous run was interrupted, remove the output directory and rebuild:
    ```bash
    rm -rf ./build/macos
    ./scripts/build_helper.sh
    ```

---

## 4) Integration with NAPS2

### How the helper is discovered and used at runtime

At export time, NAPS2 tries to embed signature fields by spawning a helper process.

1. NAPS2 searches for a bundled helper executable first:
   * See [`SignatureFieldEmbedder.FindBundledHelper()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:242).
   * It searches for the base name `naps2-signature-helper` (and also a Windows-style `.exe` name) and checks several directories relative to `AppDomain.CurrentDomain.BaseDirectory`.

2. If found, NAPS2 invokes the helper with:
   * `inputPdfPath`, `outputPdfPath`, and a JSON array of field placements.
   * See the helper invocation code in [`SignatureFieldEmbedder.EmbedFields()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:28).

### Fallback behavior (Python script)

If the helper is not found, NAPS2 logs and falls back:

* Fallback branch starts in [`SignatureFieldEmbedder.EmbedFields()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:28)
* Python executable discovery: [`SignatureFieldEmbedder.FindPythonExecutable()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:301)
* Script discovery: [`SignatureFieldEmbedder.FindScriptPath()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:356)

If Python or dependencies are missing, NAPS2 will still export a PDF, but **without embedded fields** (it copies the input PDF to the output PDF).

### Where the helper is placed in the `.app` bundle

During macOS packaging, the helper is copied into the app bundle under:

* `NAPS2.app/Contents/tools/naps2-signature-helper`

The copy logic is implemented in [`MacPackager.IncludeSignatureHelper()`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:92):

* Source path (expected build output): `build/macos/naps2-signature-helper` (see [`sourcePath`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:94))
* Destination inside bundle: `Contents/tools/naps2-signature-helper` (see [`destPath`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:102))
* The packager also runs `chmod +x` (see [`Cli.Run("chmod", …)`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:108)).

If the helper is not present at packaging time, the packager logs and continues without it.

---

## 5) Updating the Helper

### When you should rebuild

Rebuild the helper when:

* [`scripts/embed_signature_fields.py`](../scripts/embed_signature_fields.py:1) changes.
* The build configuration changes (e.g., Nuitka flags in [`build_executable()`](../scripts/build_embedder_helper.py:71)).
* The vendored pyHanko submodule changes (see [`third_party/README.md`](../third_party/README.md:29)).
* Python runtime dependencies change (see [`pyproject.toml`](../pyproject.toml:7)), especially `cryptography`, `lxml`, or `asn1crypto`.

### Updating dependencies

There are two dependency “layers” to be aware of:

1. **Vendored pyHanko source** (git submodule)
   * Update instructions: [`third_party/README.md`](../third_party/README.md:29)
   * After updating, rebuild the helper so the new vendored code is bundled.

2. **Python runtime deps** that pyHanko imports at runtime
   * These are listed as project dependencies in [`pyproject.toml`](../pyproject.toml:7).
   * If you adjust versions, rebuild the helper to pick up the changes.

### Testing an updated helper

Minimum “smoke test” after rebuild:

1. Run the helper directly on a PDF (see the verification command in [Quick verification](#quick-verification)).
2. Export a PDF from NAPS2 with signature fields on macOS and confirm NAPS2 reports it is using the bundled helper (emitted by [`SignatureFieldEmbedder.EmbedFields()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:28)).
3. Verify the output in a viewer that supports form fields.

For end-to-end testing guidance, see [`docs/SIGNATURE_FIELD_TESTING.md`](SIGNATURE_FIELD_TESTING.md:1).

---

## 6) Development Notes

### Architecture-specific builds (arm64 vs universal)

The current build script hardcodes Apple Silicon:

* [`--macos-target-arch=arm64`](../scripts/build_embedder_helper.py:106)

To produce an Intel build, you must adjust the target arch in [`scripts/build_embedder_helper.py`](../scripts/build_embedder_helper.py:1) (e.g., to `x86_64`) and rebuild.

To produce a universal build, the common approach is:

1. Build one helper for `arm64`.
2. Build one helper for `x86_64`.
3. Combine them with `lipo`.

Note: This repository currently does not provide an automated “universal helper” script; if you implement one, keep it consistent with the location expected by [`MacPackager.IncludeSignatureHelper()`](../NAPS2.Tools/Project/Packaging/MacPackager.cs:92).

### Build performance tips (ccache)

Nuitka compilation can be slow due to C compilation.

If you have `ccache` available, you can often speed up rebuilds by caching compilation artifacts:

```bash
brew install ccache
export CC="ccache clang"
export CXX="ccache clang++"
./scripts/build_helper.sh
```

### Debugging the helper

Helpful toggles live in [`scripts/build_embedder_helper.py`](../scripts/build_embedder_helper.py:1):

* Console output: the build uses [`--disable-console`](../scripts/build_embedder_helper.py:137).
  * For debugging, remove this flag so you can see stdout/stderr when launching the helper from the Finder.
* Keep intermediate build artifacts: the build uses [`--remove-output`](../scripts/build_embedder_helper.py:109).
  * For debugging build failures, temporarily remove this flag so Nuitka’s build directories remain.

At runtime, NAPS2 logs the searched paths and the chosen strategy (helper vs Python) in [`SignatureFieldEmbedder.EmbedFields()`](../NAPS2.Sdk/Pdf/SignatureFieldEmbedder.cs:28). That is typically the fastest way to diagnose “helper not found” or “execution failed” issues.
