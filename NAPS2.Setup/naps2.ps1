$SolutionRoot = ".."

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
    & (Get-MSBuild-Path) "$SolutionRoot\NAPS2.App.WinForms\NAPS2.App.WinForms.csproj" /v:q /p:Configuration=Debug | Out-Null
    $Version = [Reflection.AssemblyName]::GetAssemblyName([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.WinForms\bin\Debug\NAPS2.exe")).Version
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
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.WinForms\Properties\AssemblyInfo.cs")) $Version".*"
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.Worker\Properties\AssemblyInfo.cs")) $Version".*"
	Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.Server\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.Sdk\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.Lib.Common\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.Console\Properties\AssemblyInfo.cs")) $Version".*"
    Set-Assembly-Version ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.App.PortableLauncher\Properties\AssemblyInfo.cs")) $Version".*"
    Replace-Content ([IO.Path]::Combine($pwd, "setup.iss")) '^#define AppVersion "[^"]+"' "#define AppVersion `"$Version`""
    # TODO: Do some XML processing instead of this flaky replacement
    Replace-Content ([IO.Path]::Combine($pwd, "$SolutionRoot\NAPS2.Setup.Msi\NAPS2.Setup.Msi.wxs")) '^      Version="[^"]+"' "      Version=`"$Version`""
}

function Build-NAPS2 {
    $msbuild = Get-MSBuild-Path
    Get-Process | where { $_.ProcessName -eq "NAPS2.vshost" } | kill
	"Cleaning"
	& $msbuild "$SolutionRoot\NAPS2.sln" /v:q /t:Clean
    "Building EXE"
    & $msbuild "$SolutionRoot\NAPS2.sln" /v:q /p:Configuration=InstallerEXE
    "Building MSI"
    & $msbuild "$SolutionRoot\NAPS2.sln" /v:q /p:Configuration=InstallerMSI
    "Building Standalone"
    & $msbuild "$SolutionRoot\NAPS2.sln" /v:q /p:Configuration=Standalone
	"Build complete."
}

function Get-NAPS2-Languages {
    Get-ChildItem "$SolutionRoot\NAPS2.Sdk\Lang\po\" |
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
    $BinDir = "$SolutionRoot\NAPS2.App.WinForms\bin\$Configuration\"
    $CmdBinDir = "$SolutionRoot\NAPS2.App.Console\bin\$Configuration\"
    $ServerBinDir = "$SolutionRoot\NAPS2.App.Server\bin\$Configuration\"
    $PortableBinDir = "$SolutionRoot\NAPS2.App.PortableLauncher\bin\Release\"
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
    foreach ($File in ("$SolutionRoot\NAPS2.App.WinForms\appsettings.xml", "lib\twaindsm.dll", "lib\NAPS2.WIA.dll")) {
        cp $File $LibDir
    }
	$LibDir64 = $LibDir + "64\"
	mkdir $LibDir64
	foreach ($File in ("lib\64\twaindsm.dll", "lib\64\NAPS2.WIA.dll")) {
        cp $File $LibDir64
    }
	cp "$SolutionRoot\LICENSE" ($AppDir + "license.txt")
	cp "$SolutionRoot\CONTRIBUTORS" ($AppDir + "contributors.txt")
    if (Test-Path $ArchiveFile) {
        rm $ArchiveFile
    }
    & (Get-7z-Path) a $ArchiveFile $($StandaloneDir + "*")
    rmdir -Recurse $StandaloneDir
}
