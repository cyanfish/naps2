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
    & (Get-MSBuild-Path) ..\NAPS2.csproj /v:q /p:Configuration=Release | Out-Null
    $Version = [Reflection.AssemblyName]::GetAssemblyName([IO.Path]::Combine($pwd, "..\bin\Release\NAPS2.exe")).Version
    $VersionStr = "" + $Version.Major + "." + $Version.Minor
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
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Console\Properties\AssemblyInfo.cs")) $Version".*"
    Replace-Content ([IO.Path]::Combine($pwd, "setup.iss")) '^#define AppVersion "[^"]+"' "#define AppVersion `"$Version`""
    # TODO: Do some XML processing instead of this flaky replacement
    Replace-Content ([IO.Path]::Combine($pwd, "..\..\NAPS2.Setup\NAPS2.Setup.wxs")) '^      Version="[^"]+"' "      Version=`"$Version`""
}

function Build-NAPS2 {
    $msbuild = Get-MSBuild-Path
    Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Release
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone
}

function Get-NAPS2-Languages {
    Get-ChildItem ..\Lang\tmx\ |
        foreach { $_.Name -replace ".tmx", "" } |
        where { $_ -ne "empty" }
}
