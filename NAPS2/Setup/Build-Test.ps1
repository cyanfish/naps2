param([Parameter(Position=0, Mandatory=$false)] [String] $Name,
      [Parameter(Mandatory=$false)] [switch] $d,
      [Parameter(Mandatory=$false)] [switch] $x64)

. .\naps2.ps1

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
if ($d) {
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=StandaloneZIP /t:Rebuild /p:DefineConstants=DEBUG%3BSTANDALONE%3BSTANDALONE_ZIP
} else {
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=StandaloneZIP /t:Rebuild
}

Publish-NAPS2-Standalone $PublishDir "StandaloneZIP" ($PublishDir + "naps2-$Version-test_$Name-portable.zip") -x64:$x64

""
"Saved to " + ($PublishDir + "naps2-$Version-test_$Name-portable.zip")
""
