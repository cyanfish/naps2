; !version

[Setup]
AppName=NAPS2 (Not Another PDF Scanner 2)
AppVerName=NAPS2 {#AppVersion}
AppPublisher=Ben Olden-Cooligan
AppPublisherURL=https://www.naps2.com
AppSupportURL=https://www.naps2.com/support.html
AppUpdatesURL=https://www.naps2.com/download.html
DefaultDirName={pf}\NAPS2
DefaultGroupName=NAPS2
OutputDir=../publish/{#AppVersion}
OutputBaseFilename=naps2-{#AppVersion}-setup       
Compression=lzma
SolidCompression=yes

LicenseFile=..\..\LICENSE
UninstallDisplayIcon={app}\scanner-app.ico

[Run]
Filename: "{app}\NAPS2.exe"; Flags: nowait postinstall

[Languages]
Name: "english";              MessagesFile: "compiler:Default.isl"                                                 
Name: "Armenian";             MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Armenian.isl"; 
Name: "BrazilianPortuguese";  MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\BrazilianPortuguese.isl"; 
Name: "Bulgarian";            MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Bulgarian.isl"; 
Name: "Catalan";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Catalan.isl"; 
Name: "Corsican";             MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Corsican.isl"; 
Name: "Czech";                MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Czech.isl"; 
Name: "Danish";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Danish.isl"; 
Name: "Dutch";                MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Dutch.isl"; 
Name: "Finnish";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Finnish.isl";
Name: "French";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\French.isl";
Name: "German";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\German.isl";
Name: "Hebrew";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Hebrew.isl"; 
Name: "Icelandic";            MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Icelandic.isl"; 
Name: "Italian";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Italian.isl";
Name: "Japanese";             MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Japanese.isl";
Name: "Norwegian";            MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Norwegian.isl"; 
Name: "Polish";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Polish.isl";
Name: "Portuguese";           MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Portuguese.isl"; 
Name: "Russian";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Russian.isl"; 
Name: "Slovak";               MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Slovak.isl"; 
Name: "Slovenian";            MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Slovenian.isl"; 
Name: "Spanish";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Spanish.isl";
Name: "Turkish";              MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Turkish.isl"; 
Name: "Ukrainian";            MessagesFile: "C:\Program Files (x86)\Inno Setup 6\Languages\Ukrainian.isl"; 

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]                              
; !files

; Delete files from old locations in case of upgrade
; TODO: Delete from Program Files (x86)?
[InstallDelete]
Type: files; Name: "{app}\*.dll"
Type: filesandordirs; Name: "{app}\??"
Type: filesandordirs; Name: "{app}\??-??"

[Icons]
Name: "{group}\NAPS2"; Filename: "{app}\NAPS2.exe"
Name: "{commondesktop}\NAPS2"; Filename: "{app}\NAPS2.exe"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Action"; ValueData: "Scan with NAPS2"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "CLSID"; ValueData: "WIACLSID"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "DefaultIcon"; ValueData: "sti.dll,0"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "InitCmdLine"; ValueData: "/WiaCmd;{pf}\NAPS2\NAPS2.exe /StiDevice:%1 /StiEvent:%2;"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Provider"; ValueData: "NAPS2"

Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\StillImage\Registered Applications"; Flags:uninsdeletevalue; ValueType: string; ValueName: "NAPS2"; ValueData: "{pf}\NAPS2\NAPS2.exe"

Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Cmdline"; ValueData: "{pf}\NAPS2\NAPS2.exe /StiDevice:%1 /StiEvent:%2"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Desc"; ValueData: "Scan with NAPS2"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Icon"; ValueData: "{pf}\NAPS2\NAPS2.exe,0"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{{1c3a7177-f3a7-439e-be47-e304a185f932}"; ValueType: string; ValueName: "Name"; ValueData: "NAPS2"

