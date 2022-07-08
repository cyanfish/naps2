# NAPS2.Tools

Tools for NAPS2 building, testing, packaging, verification, etc.

## How to run

You will need the latest [.NET SDK](https://dotnet.microsoft.com/en-us/download). Open a terminal in your NAPS2 solution directory and run the following commands:

### Powershell

```
function n2 { dotnet run --project NAPS2.Tools $args }
n2 build all
```

## Commands

Run `n2 help` for a full list of commands and `n2 help <command>` for all options for a particular command. Some examples:

```
n2 clean
n2 test
n2 build exe
n2 pkg exe
n2 verify exe
```
- `clean`: Clear out bin/obj subfolders
- `test`: Run solution tests
- `build`: Builds the solution with the specified configuration (debug/exe/msi/zip/all)
- `pkg`: Generates the specified package type, e.g. exe => naps2-{version}-win-x64.exe installer
- `verify`: Installs/extracts the packaged file and runs NAPS2.App.Tests against it
  - Requires elevation for exe/msi as it does a real install on your local machine (and uninstalls the old NAPS2)
