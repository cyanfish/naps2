param([Parameter(Position=0, Mandatory=$false)] [String] $PoUrl,
      [Parameter(Position=1, Mandatory=$false)] [String] $Lang)

. .\naps2.ps1

# Download PO file and update language files

$Ext = [System.IO.Path]::GetExtension($PoUrl)
if (-not ($Ext -eq ".po")) {
    return
}

$LanguageCode = $Lang
if ([string]::IsNullOrEmpty($LanguageCode)) {
	$LanguageCode = [System.IO.Path]::GetFileNameWithoutExtension($PoUrl)
	if ($LanguageCode -eq $null -or $LanguageCode -eq "") {
		return
	}
}
Invoke-WebRequest -Uri $PoUrl -OutFile "..\..\NAPS2.Core\Lang\po\$LanguageCode.po"
.\Update-Resources.ps1 $LanguageCode

# Rebuild NAPS2

$Version = Get-NAPS2-Version
$PublishDir = "..\publish\$Version\"
if (-not (Test-Path $PublishDir)) {
    mkdir $PublishDir
}
$msbuild = Get-MSBuild-Path
Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill
"Building MSI"
& $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=InstallerMSI
"Building ZIP"
& $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone

Publish-NAPS2-Standalone $PublishDir "Standalone" ($PublishDir + "naps2-$Version-test_$LanguageCode-portable.zip")

""
"Saved to " + ($PublishDir + "naps2-$Version-test_$LanguageCode-portable.zip")
""

$confirmation = Read-Host "Revert changes (y/n)"
if ($confirmation -eq 'y') {
    & git checkout -- "..\..\*.po"
    & git checkout -- "..\..\*.resx"
}