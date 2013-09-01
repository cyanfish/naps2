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

# Standalone ZIP
$StandaloneDir = $PublishDir + "naps2-$Version-standalone\"
$StandaloneZip = $PublishDir + "naps2-$Version-standalone.zip"
$Standalone7z = $PublishDir + "naps2-$Version-standalone.7z"
if (Test-Path $StandaloneDir) {
    rmdir $StandaloneDir -Recurse
}
mkdir $StandaloneDir
$BinDir = "..\bin\Standalone\"
$CmdBinDir = "..\..\NAPS2.Console\bin\Standalone\"
foreach ($LanguageCode in Get-NAPS2-Languages) {
    $LangDir = $StandaloneDir + "$LanguageCode\"
    mkdir $LangDir
    cp ($BinDir + "$LanguageCode\NAPS2.resources.dll") $LangDir
}
foreach ($Dir in ($BinDir, $CmdBinDir)) {
    foreach ($File in (Get-ChildItem $Dir | where { $_.Name -match '(?<!vshost)\.(exe|dll)$' })) {
        cp $File.FullName $StandaloneDir
    }
}
foreach ($File in ("..\Resources\scanner-app.ico", "..\appsettings.xml", "lib\wiaaut.dll", "license.txt")) {
    cp $File $StandaloneDir
}
if (Test-Path $StandaloneZip) {
    rm $StandaloneZip
}
if (Test-Path $Standalone7z) {
    rm $Standalone7z
}
& (Get-7z-Path) a $StandaloneZip $($StandaloneDir + "*")
& (Get-7z-Path) a $Standalone7z $($StandaloneDir + "*")
rmdir -Recurse $StandaloneDir
