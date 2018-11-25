param()

. .\naps2.ps1

$msbuild = Get-MSBuild-Path
"Building NAPS2.WIA in Debug mode"
& $msbuild ..\..\NAPS2.WIA\NAPS2.WIA.vcxproj /v:q /p:"Configuration=Debug;Platform=Win32"
& $msbuild ..\..\NAPS2.WIA\NAPS2.WIA.vcxproj /v:q /p:"Configuration=Debug;Platform=x64"
"Building NAPS2.WIA in Release mode"
& $msbuild ..\..\NAPS2.WIA\NAPS2.WIA.vcxproj /v:q /p:"Configuration=Release;Platform=Win32"
& $msbuild ..\..\NAPS2.WIA\NAPS2.WIA.vcxproj /v:q /p:"Configuration=Release;Platform=x64"
"Done"
