param([Parameter(Position=0, Mandatory=$false)] [String] $Version,
      [Switch] $Force)

. .\naps2.ps1

if ($Version -notmatch '^[0-9]+(\.[0-9]+)+$') {
    $Version = Get-NAPS2-Version
}

$PublishDir = "..\publish\$Version\"
if (-not (Test-Path $PublishDir)) {
    mkdir $PublishDir
} elseif (-not $Force) {
    "Publish dir already exists: $PublishDir"
    "Use -Force to overwrite."
    return
}

Set-NAPS2-Version $Version
Build-NAPS2

# MSI Installer
cp "..\..\NAPS2.Setup\bin\Release\NAPS2.Setup.msi" ($PublishDir + "naps2-$Version-setup.msi")

# EXE Installer
& (Get-Inno-Path) "setup.iss"

# Standalone ZIP/7Z
Publish-NAPS2-Standalone $PublishDir "StandaloneZIP" ($PublishDir + "naps2-$Version-portable.zip")
Publish-NAPS2-Standalone $PublishDir "Standalone7Z" ($PublishDir + "naps2-$Version-portable.7z")