#!/usr/bin/env python3

# Forked from: https://github.com/flatpak/flatpak-builder-tools/blob/master/dotnet/flatpak-dotnet-generator.py
__license__ = 'MIT'

from pathlib import Path

import argparse
import base64
import binascii
import json
import subprocess
import tempfile


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

        def runCommand(cmd):
            subprocess.run([
                'flatpak', 'run',
                '--env=DOTNET_CLI_TELEMETRY_OPTOUT=true',
                '--env=DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true',
                '--command=sh', '--runtime=org.freedesktop.Sdk//22.08', '--share=network',
                '--filesystem=host', 'org.freedesktop.Sdk.Extension.dotnet6//22.08', '-c',
                'PATH="${PATH}:/usr/lib/sdk/dotnet6/bin" NUGET_PACKAGES="' +
                str(Path(tmp).resolve()) +
                '" LD_LIBRARY_PATH="$LD_LIBRARY_PATH:/usr/lib/sdk/dotnet6/lib" exec dotnet ' + cmd,
                '--', args.project] + runtime_args)

        runCommand('restore -r linux-x64 "$@"')
        runCommand('restore -r linux-arm64 "$@"')

        for path in Path(tmp).glob('**/*.nupkg.sha512'):
            name = path.parent.parent.name
            version = path.parent.name
            filename = '{}.{}.nupkg'.format(name, version)
            url = 'https://api.nuget.org/v3-flatcontainer/{}/{}/{}'.format(name, version,
                                                                           filename)

            with path.open() as fp:
                sha512 = binascii.hexlify(base64.b64decode(fp.read())).decode('ascii')

            sources.append({
                'type': 'file',
                'url': url,
                'sha512': sha512,
                'dest': args.destdir,
                'dest-filename': filename,
            })

    with open(args.output, 'w') as fp:
        json.dump(
            sorted(sources, key=lambda n: n.get("dest-filename")),
            fp,
            indent=4
        )


if __name__ == '__main__':
    main()
