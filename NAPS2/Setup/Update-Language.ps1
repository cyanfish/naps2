param([Parameter(Position=0, Mandatory=$false)] [String] $LanguageCode)

. .\naps2.ps1

function Update-Lang {
    param([Parameter(Position=0)] [String] $LanguageCode)
    foreach ($ResourceFolder in ("..\..\NAPS2.Core\Lang\Resources\", "..\..\NAPS2.Core\WinForms\")) {
        foreach ($ResourceFile in (Get-ChildItem $ResourceFolder | where { $_.Name -match '^[a-zA-Z-]+\.resx$' })) {
            & "C:\Program Files\RTT\RTT64.exe" /S"$($ResourceFile.FullName)" /T"$($ResourceFile.FullName -replace '\.resx$', ".$LanguageCode.resx" )" /M"..\..\NAPS2.Core\Lang\po\$LanguageCode.po" /O"NUL"
        }
    }
}

if ($LanguageCode -eq $null -or $LanguageCode -eq "") {
    foreach ($LanguageCode in Get-NAPS2-Languages) {
        Update-Lang $LanguageCode
    }
} else {
    Update-Lang $LanguageCode
}
