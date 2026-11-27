#!/usr/bin/env python3

# Forked from: https://github.com/flatpak/flatpak-builder-tools/blob/master/dotnet/flatpak-dotnet-generator.py
__license__ = 'MIT'

from pathlib import Path

import argparse
import base64
import binascii
import hashlib
import json
import os
import subprocess
import tempfile
import urllib.request


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('output', help='The output JSON sources file')
    parser.add_argument('project', help='The project file')
    parser.add_argument('--runtime', '-r', help='The target runtime to restore packages for')
    parser.add_argument('--destdir',
                        help='The directory the generated sources file will save sources to',
                        default='nuget-sources')
    args = parser.parse_args()

    sources = []

    with tempfile.TemporaryDirectory(dir=Path()) as tmp:
        runtime_args = []
        if args.runtime:
            runtime_args.extend(('-r', args.runtime))

        # "flatpak run" forwards the host environment into the sandbox. When this script is launched via "dotnet run",
        # that includes MSBuild/.NET variables pointing at the host SDK, which doesn't exist in the sandbox.
        env = {k: v for k, v in os.environ.items() if not k.upper().startswith(('MSBUILD', 'DOTNET_'))}

        def runCommand(cmd):
            subprocess.run([
                'flatpak', 'run',
                '--env=DOTNET_CLI_TELEMETRY_OPTOUT=true',
                '--env=DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true',
                '--command=sh', '--runtime=org.freedesktop.Sdk//26.08', '--share=network',
                '--filesystem=host', 'org.freedesktop.Sdk.Extension.dotnet10//26.08', '-c',
                'PATH="${PATH}:/usr/lib/sdk/dotnet10/bin" NUGET_PACKAGES="' +
                str(Path(tmp).resolve()) +
                '" LD_LIBRARY_PATH="$LD_LIBRARY_PATH:/usr/lib/sdk/dotnet10/lib" exec dotnet ' + cmd,
                '--', args.project] + runtime_args, env=env, check=True)

        runCommand('restore -r linux-x64 "$@"')
        runCommand('restore -r linux-arm64 "$@"')

        packages = set()
        for path in Path(tmp).glob('**/*.nupkg.sha512'):
            name = path.parent.parent.name
            version = path.parent.name
            packages.add((name, version))

            with path.open() as fp:
                sha512 = binascii.hexlify(base64.b64decode(fp.read())).decode('ascii')

            sources.append(make_source(name, version, sha512, args.destdir))

        # ReadyToRun compilation uses a crossgen2 package for the build machine's architecture, so restoring here only
        # pulls the one for this machine. The flatpak is built on both architectures, so add the other one too.
        crossgen_prefix = 'microsoft.netcore.app.crossgen2.'
        crossgen_versions = {version for name, version in packages if name.startswith(crossgen_prefix)}
        for version in sorted(crossgen_versions):
            for arch in ('linux-x64', 'linux-arm64'):
                name = crossgen_prefix + arch
                if (name, version) not in packages:
                    with urllib.request.urlopen(get_url(name, version)) as response:
                        sha512 = hashlib.sha512(response.read()).hexdigest()
                    sources.append(make_source(name, version, sha512, args.destdir))

    with open(args.output, 'w') as fp:
        json.dump(
            sorted(sources, key=lambda n: n.get("dest-filename")),
            fp,
            indent=4
        )


def get_url(name, version):
    return 'https://api.nuget.org/v3-flatcontainer/{}/{}/{}.{}.nupkg'.format(name, version, name, version)


def make_source(name, version, sha512, destdir):
    return {
        'type': 'file',
        'url': get_url(name, version),
        'sha512': sha512,
        'dest': destdir,
        'dest-filename': '{}.{}.nupkg'.format(name, version),
    }


if __name__ == '__main__':
    main()
