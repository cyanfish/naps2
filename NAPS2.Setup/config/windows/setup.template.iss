; !defs

; Set up for InnoDependencyInstaller
#define public Dependency_NoExampleSetup
#include "..\config\windows\CodeDependencies.iss"
#include "..\config\windows\setup.template.languages.iss"

#define AppShortName             "NAPS"
#define AppLongName              "NAPS2 - Not Another PDF Scanner"
#define AppCompany               "NAPS2 Software"
#define AppCopyrightStartingYear "2009"
#define AppCopyrightEndYear      GetDateTimeString('yyyy','','')
#define AppCopyrightCompany      "NAPS2 Contributors"

[Setup]
AppName={#AppLongName}
AppVersion={#AppVersion}
AppVerName={#AppShortName} {#AppVersionName}

AppPublisher={#AppCompany}
AppPublisherURL=https://www.naps2.com
AppSupportURL=https://www.naps2.com/support
AppUpdatesURL=https://www.naps2.com/download

VersionInfoDescription={#AppShortName} installer
VersionInfoVersion={#AppVersion}
VersionInfoProductName={#AppShortName}
VersionInfoProductVersion={#AppVersion}

VersionInfoCompany={#AppCompany}
VersionInfoCopyright=(c) {#AppCopyrightStartingYear}-{#AppCopyrightEndYear}

UninstallDisplayName={#AppShortName}
UninstallDisplayIcon={app}\NAPS2.exe

ShowLanguageDialog=yes
UsePreviousLanguage=no
LanguageDetectionMethod=uilanguage
WizardStyle=modern

DefaultDirName={commonpf}\{#AppShortName}
DefaultGroupName={#AppShortName}
OutputDir=../publish/{#AppVersionName}
OutputBaseFilename=naps2-{#AppVersionName}-{#AppPlatform}
Compression=lzma2/ultra
LZMAUseSeparateProcess=yes
SolidCompression=yes
; !arch

LicenseFile=..\..\LICENSE

[Run]
Filename: "{app}\NAPS2.exe"; Flags: nowait postinstall

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; Impl for InnoDependencyInstaller
[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddVC2015To2022;
  Result := True;
end;

[Files]                              
; !files

; Delete files from old locations in case of upgrade
[InstallDelete]     
Type: files; Name: "{app}\*.exe"
Type: files; Name: "{app}\*.exe.config"
Type: filesandordirs; Name: "{app}\lib"
; !clean32

[Icons]
Name: "{group}\NAPS2"; Filename: "{app}\NAPS2.exe"
Name: "{commondesktop}\NAPS2"; Filename: "{app}\NAPS2.exe"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Action"; ValueData: "Scan with NAPS2"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "CLSID"; ValueData: "WIACLSID"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "DefaultIcon"; ValueData: "sti.dll,0"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "InitCmdLine"; ValueData: "/WiaCmd;{app}\NAPS2.exe /StiDevice:%1 /StiEvent:%2;"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Provider"; ValueData: "NAPS2"

Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\StillImage\Registered Applications"; Flags:uninsdeletevalue; ValueType: string; ValueName: "NAPS2"; ValueData: "{app}\NAPS2.exe"

Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Cmdline"; ValueData: "{app}\NAPS2.exe /StiDevice:%1 /StiEvent:%2"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Desc"; ValueData: "Scan with NAPS2"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\NAPS2.exe,0"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Name"; ValueData: "NAPS2"

