# Third-Party Dependencies

This directory contains vendored third-party dependencies for NAPS2.

## pyHanko

**Purpose**: PDF signature field embedding  
**Version**: Git submodule at commit b89f139e5c5e0f9895a39686ff2dc4c74dd23ba8  
**License**: MIT  
**Repository**: https://github.com/MatthiasValvekens/pyHanko  
**Documentation**: https://docs.pyhanko.eu/

### Why Vendored?

pyHanko is vendored as a git submodule to:
1. Ensure consistent behavior across installations
2. Avoid requiring users to `pip install pyHanko` separately
3. Pin to a specific, tested version
4. Simplify the build and deployment process

### Usage

The [`scripts/embed_signature_fields.py`](../scripts/embed_signature_fields.py) script automatically adds the vendored pyHanko source to the Python path. Users only need to install pyHanko's runtime dependencies:

```bash
pip install -r scripts/requirements-signature-fields.txt
```

### Updating pyHanko

To update the vendored pyHanko to a newer version:

```bash
cd third_party/pyHanko
git fetch origin
git checkout <new-commit-or-tag>
cd ../..
git add third_party/pyHanko
git commit -m "Update pyHanko to <version>"
```

### Initializing Submodules

When cloning the NAPS2 repository, initialize the submodules:

```bash
git clone <naps2-repo-url>
cd naps2
git submodule update --init --recursive
```

## License Information

Each vendored dependency retains its original license. See the LICENSE file in each subdirectory for details.
