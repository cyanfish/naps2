param([Parameter(Position=0, Mandatory=$false)] [String] $LanguageCode)

. .\naps2.ps1

$Localize = "$SolutionRoot\NAPS2.Tools.Localization\bin\Debug\NAPS2.Localization.exe"

if (Test-Path $Localize) {
	if ($LanguageCode -eq $null -or $LanguageCode -eq "") {
		foreach ($LanguageCode in Get-NAPS2-Languages) {
			"$LanguageCode"
			& $Localize language $LanguageCode
		}
	} else {
		"$LanguageCode"
		& $Localize language $LanguageCode
	}
} else {
	"NAPS2.Tools.Localization is not built"
}
