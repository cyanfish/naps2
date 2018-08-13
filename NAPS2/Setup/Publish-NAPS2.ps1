param([Parameter(Position=0, Mandatory=$false)] [String] $Version,
      [Parameter(Mandatory=$false)] [String] $Name,
      [Switch] $Force)

. .\naps2.ps1

if ($Version -notmatch '^[0-9]+(\.[0-9]+)+$') {
    $Version = Get-NAPS2-Version
}

if ([string]::IsNullOrEmpty($Name)) {
    $Name = $Version
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
cp "..\..\NAPS2.Setup\bin\Release\NAPS2.Setup.msi" ($PublishDir + "naps2-$Name-setup.msi")

# EXE Installer
& (Get-Inno-Path) "setup.iss"
if (-not [string]::IsNullOrEmpty($Name)) {
	if ($Force -and (Test-Path ($PublishDir + "naps2-$Name-setup.exe")) -and (-not ($Name -eq $Version))) {
		Remove-Item ($PublishDir + "naps2-$Name-setup.exe")
	}
	ren ($PublishDir + "naps2-$Version-setup.exe") "naps2-$Name-setup.exe"
}

# Standalone ZIP/7Z
Publish-NAPS2-Standalone $PublishDir "Standalone" ($PublishDir + "naps2-$Name-portable.zip")
Publish-NAPS2-Standalone $PublishDir "Standalone" ($PublishDir + "naps2-$Name-portable.7z")