@echo off
if "%1"=="" goto noarg
del NAPS2\Lang\Resources\%1\*.*.resx
rename NAPS2\Lang\Resources\%1\*.resx *.%1.resx
copy /Y NAPS2\Lang\Resources\%1\* NAPS2\Lang\Resources\
rmdir /S /Q NAPS2\Lang\Resources\%1\
del NAPS2\WinForms\%1\*.*.resx
rename NAPS2\WinForms\%1\*.resx *.%1.resx
copy /Y NAPS2\WinForms\%1\* NAPS2\WinForms\
rmdir /S /Q NAPS2\WinForms\%1\
goto end
:noarg
echo Usage (for language xx): updatelang xx
:end