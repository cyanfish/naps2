param([Parameter(Position=0, Mandatory=$false)] [String] $LanguageCode)

. .\naps2.ps1

if ($LanguageCode -eq $null -or $LanguageCode -eq "") {
    foreach ($LanguageCode in Get-NAPS2-Languages) {
        Update-Lang $LanguageCode
    }
} else {
    Update-Lang $LanguageCode
}
