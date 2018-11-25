. .\naps2.ps1

$Localize = "..\..\NAPS2.Localization\bin\Debug\NAPS2.Localization.exe"

if (Test-Path $Localize) {
	& $Localize templates
} else {
	"NAPS2.Localization is not built"
}
