param([Parameter(Position=0, Mandatory=$false)] [String] $Name,
      [Parameter(Mandatory=$false)] [switch] $d)

. .\naps2.ps1

# Rebuild NAPS2

$msbuild = Get-MSBuild-Path
& $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Debug

$Version = Get-NAPS2-Version
$PublishDir = "..\publish\$Version\"
if (-not (Test-Path $PublishDir)) {
    mkdir $PublishDir
}
Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill

"Building ZIP"
& $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone /t:Clean
if ($d) {
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone /t:Rebuild /p:DefineConstants=DEBUG%3BSTANDALONE
} else {
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone /t:Rebuild
}

Publish-NAPS2-Standalone $PublishDir "Standalone" ($PublishDir + "naps2-$Version-test_$Name-portable.zip")

""
"Saved to " + ($PublishDir + "naps2-$Version-test_$Name-portable.zip")
""
