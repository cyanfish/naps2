function Get-MSBuild-Path {
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
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
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Worker\Properties\AssemblyInfo.cs")) $Version".*"
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Server\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Core\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Console\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Portable\Properties\AssemblyInfo.cs")) $Version".*"
    Replace-Content ([IO.Path]::Combine($pwd, "setup.iss")) '^#define AppVersion "[^"]+"' "#define AppVersion `"$Version`""
    # TODO: Do some XML processing instead of this flaky replacement
    Replace-Content ([IO.Path]::Combine($pwd, "..\..\NAPS2.Setup\NAPS2.Setup.wxs")) '^      Version="[^"]+"' "      Version=`"$Version`""
}

function Build-NAPS2 {
    $msbuild = Get-MSBuild-Path
    Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill
	"Cleaning"
	& $msbuild ..\..\NAPS2.sln /v:q /t:Clean
    "Building EXE"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=InstallerEXE
    "Building MSI"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=InstallerMSI
    "Building Standalone"
    & $msbuild ..\..\NAPS2.sln /v:q /p:Configuration=Standalone
	"Build complete."
}

function Get-NAPS2-Languages {
    Get-ChildItem ..\..\NAPS2.Core\Lang\po\ |
        foreach { $_.Name -replace ".po", "" } |
        where { $_ -ne "templatest" } |
        where { $_ -ne "en" }
}

function Publish-NAPS2-Standalone {
    param([Parameter(Position=0)] [String] $PublishDir,
	      [Parameter(Position=1)] [String] $Configuration,
          [Parameter(Position=2)] [String] $ArchiveFile)
	$StandaloneDir = $PublishDir + "naps2-$Version-portable\"
    $AppDir = $StandaloneDir + "App\"
	$LibDir = $StandaloneDir + "App\lib\"
    $DataDir = $StandaloneDir + "Data\"
    if (Test-Path $StandaloneDir) {
        rmdir $StandaloneDir -Recurse
    }
    mkdir $StandaloneDir
    mkdir $AppDir
    mkdir $DataDir
    $BinDir = "..\bin\$Configuration\"
    $CmdBinDir = "..\..\NAPS2.Console\bin\$Configuration\"
    $ServerBinDir = "..\..\NAPS2.Server\bin\$Configuration\"
    $PortableBinDir = "..\..\NAPS2.Portable\bin\Release\"
    cp ($PortableBinDir + "NAPS2.Portable.exe") $StandaloneDir
    foreach ($LanguageCode in Get-NAPS2-Languages) {
        $LangDir = $LibDir + "$LanguageCode\"
        mkdir $LangDir
        cp ($BinDir + "$LanguageCode\NAPS2.Core.resources.dll") $LangDir
    }
    foreach ($Dir in ($BinDir, $CmdBinDir)) {
        foreach ($File in (Get-ChildItem $Dir | where { $_.Name -match '(?<!vshost)\.exe(\.config)?$' })) {
            cp $File.FullName $AppDir
        }
        foreach ($File in (Get-ChildItem $Dir | where { $_.Name -match '\.dll$' })) {
            cp $File.FullName $LibDir
        }
    }
    foreach ($File in ("..\appsettings.xml", "lib\twaindsm.dll", "lib\NAPS2.WIA.dll")) {
        cp $File $LibDir
    }
	$LibDir64 = $LibDir + "64\"
	mkdir $LibDir64
	foreach ($File in ("lib\64\twaindsm.dll", "lib\64\NAPS2.WIA.dll")) {
        cp $File $LibDir64
    }
	cp "..\..\LICENSE" ($AppDir + "license.txt")
	cp "..\..\CONTRIBUTORS" ($AppDir + "contributors.txt")
    if (Test-Path $ArchiveFile) {
        rm $ArchiveFile
    }
    & (Get-7z-Path) a $ArchiveFile $($StandaloneDir + "*")
    rmdir -Recurse $StandaloneDir
}
