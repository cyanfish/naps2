function Get-MSBuild-Path {
    "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
}

function Get-7z-Path {
    "C:\Program Files\7-Zip\7z.exe"
}

function Get-Inno-Path {
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
}

function Get-NAPS2-Version {
    & (Get-MSBuild-Path) ..\NAPS2.csproj /v:q /p:Configuration=Debug | Out-Null
    $Version = [Reflection.AssemblyName]::GetAssemblyName([IO.Path]::Combine($pwd, "..\bin\StandaloneZIP\NAPS2.exe")).Version
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
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2_64\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Core\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2.Console\Properties\AssemblyInfo.cs")) $Version".*"
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "..\..\NAPS2_64.Console\Properties\AssemblyInfo.cs")) $Version".*"
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
          [Parameter(Position=2)] [String] $ArchiveFile,
		  [Parameter(Mandatory=$false)] [switch] $x64)
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
    $BinDir64 = "..\..\NAPS2_64\bin\Release\"
    $CmdBinDir = "..\..\NAPS2.Console\bin\$Configuration\"
    $CmdBinDir64 = "..\..\NAPS2_64.Console\bin\Release\"
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
	if ($x64) {
		foreach ($Dir in ($BinDir64, $CmdBinDir64)) {
			foreach ($File in (Get-ChildItem $Dir | where { $_.Name -match '(?<!vshost)\.exe(\.config)?$' })) {
				cp $File.FullName $AppDir
			}
		}
		$LibDir64 = $LibDir + "64\"
		mkdir $LibDir64
		cp "lib\64\twaindsm.dll" $LibDir64
	}
    foreach ($File in ("..\appsettings.xml", "lib\twaindsm.dll", "lib\wiaaut.dll")) {
        cp $File $AppDir
    }
	cp "..\..\LICENSE" ($AppDir + "license.txt")
	cp "..\..\CONTRIBUTORS" ($AppDir + "contributors.txt")
    if (Test-Path $ArchiveFile) {
        rm $ArchiveFile
    }
    & (Get-7z-Path) a $ArchiveFile $($StandaloneDir + "*")
    rmdir -Recurse $StandaloneDir
}

function Update-Lang {
    param([Parameter(Position=0)] [String] $LanguageCode)
    foreach ($ResourceFolder in ("..\..\NAPS2.Core\Lang\Resources\", "..\..\NAPS2.Core\WinForms\")) {
        foreach ($ResourceFile in (Get-ChildItem $ResourceFolder | where { $_.Name -match '^[a-zA-Z-]+\.resx$' })) {
            & "C:\Program Files\RTT\RTT64.exe" /S"$($ResourceFile.FullName)" /T"$($ResourceFile.FullName -replace '\.resx$', ".$LanguageCode.resx" )" /M"..\..\NAPS2.Core\Lang\po\$LanguageCode.po" /O"NUL"
        }
    }
}