function Get-MSBuild-Path {
    [IO.Path]::Combine((Get-ItemProperty -path HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0).MSBuildToolsPath, "msbuild.exe")
}

function Get-7z-Path {
    "C:\Program Files\7-Zip\7z.exe"
}

function Get-Inno-Path {
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
}

function Get-NAPS2-Version {
    & (Get-MSBuild-Path) ..\NAPS2.csproj /v:q /p:Configuration=Debug | Out-Null
    $Version = [Reflection.AssemblyName]::GetAssemblyName([IO.Path]::Combine($pwd, "..\bin\Debug\NAPS2.exe")).Version
    $VersionStr = "" + $Version.Major + "." + $Version.Minor + "." + $Version.Build
    $VersionStr
}

function Replace-Content {
    param([Parameter(Position=0)] [String] $Path,
          [Parameter(Position=1)] [String] $SearchRegex,
          [Parameter(Position=2)] [String] $Replacement)
    (Get-Content $Path) |
        foreach { $_ -replace $SearchRegex, $Replacement } |
            Set-Content $Path
}

function Set-Assembly-Version {
    param([Parameter(Position=0)] [String] $AssemblyInfoPath,
          [Parameter(Position=1)] [String] $Version)
    Replace-Content $AssemblyInfoPath 'AssemblyVersion\("[^\)]+"\)' "AssemblyVersion(`"$Version`")"
}

function Set-NAPS2-Version {
    param([Parameter(Position=0)] [String] $Version)
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Core\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Console\Properties\AssemblyInfo.cs")) $Version".*"
    Replace-Content ([IO.Path]::Combine($pwd, "setup.iss")) '^#define AppVersion "[^"]+"' "#define AppVersion `"$Version`""
    # TODO: Do some XML processing instead of this flaky replacement
    Replace-Content ([IO.Path]::Combine($pwd, "..\..\NAPS2.Setup\NAPS2.Setup.wxs")) '^      Version="[^"]+"' "      Version=`"$Version`""
}

function Build-NAPS2 {
    $msbuild = Get-MSBuild-Path
    Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill
    "Building EXE"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=InstallerEXE
    "Building MSI"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=InstallerMSI
    "Building ZIP"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=StandaloneZIP
    "Building 7Z"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone7Z
}

function Get-NAPS2-Languages {
    Get-ChildItem ..\..\NAPS2.Core\Lang\po\ |
        foreach { $_.Name -replace ".po", "" } |
        where { $_ -ne "templatest" } |
        where { $_ -ne "en" }
}
