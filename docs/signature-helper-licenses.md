# Signature Helper - License Analysis

This document provides a comprehensive analysis of all licenses for components bundled in the NAPS2 signature helper executable.

## Summary

The NAPS2 signature helper is a standalone executable that bundles several open-source components. All bundled components use permissive licenses (MIT, Apache 2.0, BSD) that are compatible with NAPS2's GPLv2 license.

## License Compatibility

**NAPS2 License**: GNU General Public License v2.0 (GPLv2)

**Bundled Components**: All use permissive licenses (MIT, Apache 2.0, BSD) which are compatible with GPLv2. The GPLv2 allows linking with MIT/Apache/BSD-licensed code, and the resulting combined work is distributed under GPLv2.

## Component Licenses

### 1. pyHanko
- **License**: MIT License
- **Copyright**: Copyright (c) 2020-2023 Matthias Valvekens
- **Source**: https://github.com/MatthiasValvekens/pyHanko
- **License File**: [`third_party/pyHanko/LICENSE`](../third_party/pyHanko/LICENSE)
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 2. pyhanko-certvalidator
- **License**: MIT License
- **Copyright**: 
  - Copyright (c) 2015-2018 Will Bond <will@wbond.net>
  - Copyright (c) 2020-2023 Matthias Valvekens <dev@mvalvekens.be>
- **Source**: https://github.com/MatthiasValvekens/pyHanko (subpackage)
- **License File**: [`third_party/pyHanko/pkgs/pyhanko-certvalidator/LICENSE`](../third_party/pyHanko/pkgs/pyhanko-certvalidator/LICENSE)
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 3. asn1crypto
- **License**: MIT License
- **Copyright**: Copyright (c) 2015-2024 Will Bond <will@wbond.net>
- **Source**: https://github.com/wbond/asn1crypto
- **PyPI**: https://pypi.org/project/asn1crypto/
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 4. cryptography
- **License**: Apache License 2.0 and BSD License (dual-licensed)
- **Copyright**: Copyright (c) Individual contributors
- **Source**: https://github.com/pyca/cryptography
- **PyPI**: https://pypi.org/project/cryptography/
- **Compatibility**: ✅ Apache 2.0 and BSD are compatible with GPLv2

### 5. lxml
- **License**: BSD License
- **Copyright**: Copyright (c) 2004 Infrae
- **Source**: https://github.com/lxml/lxml
- **PyPI**: https://pypi.org/project/lxml/
- **Compatibility**: ✅ BSD is compatible with GPLv2

### 6. oscrypto
- **License**: MIT License
- **Copyright**: Copyright (c) 2015-2018 Will Bond <will@wbond.net>
- **Source**: https://github.com/wbond/oscrypto
- **PyPI**: https://pypi.org/project/oscrypto/
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 7. requests
- **License**: Apache License 2.0
- **Copyright**: Copyright 2019 Kenneth Reitz
- **Source**: https://github.com/psf/requests
- **PyPI**: https://pypi.org/project/requests/
- **Compatibility**: ✅ Apache 2.0 is compatible with GPLv2

### 8. tzlocal
- **License**: MIT License
- **Copyright**: Copyright (c) 2011-2017 Lennart Regebro
- **Source**: https://github.com/regebro/tzlocal
- **PyPI**: https://pypi.org/project/tzlocal/
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 9. pyyaml
- **License**: MIT License
- **Copyright**: Copyright (c) 2017-2021 Ingy döt Net, Copyright (c) 2006-2016 Kirill Simonov
- **Source**: https://github.com/yaml/pyyaml
- **PyPI**: https://pypi.org/project/PyYAML/
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 10. uritools
- **License**: MIT License
- **Copyright**: Copyright (c) 2014-2024 Thomas Kemmer
- **Source**: https://github.com/tkem/uritools
- **PyPI**: https://pypi.org/project/uritools/
- **Compatibility**: ✅ MIT is compatible with GPLv2

### 11. Nuitka (Build Tool Only)
- **License**: Apache License 2.0
- **Copyright**: Copyright (c) Kay Hayen and Nuitka Contributors
- **Source**: https://github.com/Nuitka/Nuitka
- **Note**: Nuitka is only used as a build tool to compile Python to C. The resulting executable does not contain Nuitka code, only the compiled application code and Python runtime.
- **Compatibility**: ✅ Apache 2.0 is compatible with GPLv2

## License Obligations

### MIT License Requirements
For all MIT-licensed components (pyHanko, pyhanko-certvalidator, asn1crypto, oscrypto, tzlocal, pyyaml, uritools):
- ✅ **Attribution**: Copyright notices are preserved in source code
- ✅ **License Text**: MIT license text is included in the repository
- ✅ **No Warranty**: MIT license includes no warranty clause

### Apache 2.0 License Requirements
For Apache 2.0-licensed components (cryptography, requests):
- ✅ **Attribution**: Copyright notices are preserved
- ✅ **License Text**: Apache 2.0 license is referenced
- ✅ **Notice File**: Any NOTICE files from dependencies are preserved
- ✅ **Patent Grant**: Apache 2.0 includes explicit patent grant

### BSD License Requirements
For BSD-licensed components (lxml):
- ✅ **Attribution**: Copyright notices are preserved
- ✅ **License Text**: BSD license text is included
- ✅ **No Endorsement**: BSD license includes no endorsement clause

## Distribution Requirements

When distributing the NAPS2 signature helper executable:

1. **Source Code Availability** (GPLv2 requirement):
   - ✅ Source code is available at: https://github.com/ronnyhopf/naps2
   - ✅ Build instructions are provided in [`docs/signature-helper-build.md`](signature-helper-build.md)
   - ✅ All dependencies are documented in [`scripts/requirements-signature-fields.txt`](../scripts/requirements-signature-fields.txt)

2. **License Notices**:
   - ✅ NAPS2 LICENSE file (GPLv2) is included in distributions
   - ✅ Third-party licenses are preserved in the repository
   - ✅ This license analysis document provides comprehensive attribution

3. **Combined Work License**:
   - The signature helper executable is a combined work under GPLv2
   - All bundled MIT/Apache/BSD components remain under their original licenses
   - The combined work is distributed under GPLv2 terms

## Conclusion

All components bundled in the NAPS2 signature helper use permissive open-source licenses (MIT, Apache 2.0, BSD) that are fully compatible with NAPS2's GPLv2 license. The distribution complies with all license requirements:

- ✅ All copyright notices are preserved
- ✅ All license texts are available in the repository
- ✅ Source code is publicly available
- ✅ Build instructions are documented
- ✅ No license conflicts exist

The signature helper can be legally distributed as part of NAPS2 under the GPLv2 license.

## References

- [GNU GPL Compatibility](https://www.gnu.org/licenses/license-list.html)
- [MIT License](https://opensource.org/licenses/MIT)
- [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)
- [BSD License](https://opensource.org/licenses/BSD-3-Clause)
- [NAPS2 License](../LICENSE)
