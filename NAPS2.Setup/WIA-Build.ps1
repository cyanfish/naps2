param()

. .\naps2.ps1

$msbuild = Get-MSBuild-Path
"Building NAPS2.WIA in Debug mode"
& $msbuild "$SolutionRoot\NAPS2.Sdk.NativeWia\NAPS2.Sdk.NativeWia.vcxproj" /v:q /p:"Configuration=Debug;Platform=Win32"
& $msbuild "$SolutionRoot\NAPS2.Sdk.NativeWia\NAPS2.Sdk.NativeWia.vcxproj" /v:q /p:"Configuration=Debug;Platform=x64"
"Building NAPS2.WIA in Release mode"
& $msbuild "$SolutionRoot\NAPS2.Sdk.NativeWia\NAPS2.Sdk.NativeWia.vcxproj" /v:q /p:"Configuration=Release;Platform=Win32"
& $msbuild "$SolutionRoot\NAPS2.Sdk.NativeWia\NAPS2.Sdk.NativeWia.vcxproj" /v:q /p:"Configuration=Release;Platform=x64"
"Done"
