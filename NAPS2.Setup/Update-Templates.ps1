. .\naps2.ps1

$Localize = "$SolutionRoot\NAPS2.Tools.Localization\bin\Debug\NAPS2.Localization.exe"

if (Test-Path $Localize) {
	& $Localize templates
} else {
	"NAPS2.Tools.Localization is not built"
}
