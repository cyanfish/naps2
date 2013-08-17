@echo off
cd %~dp0
set VERSION=2.4
set OUTFOLDER=..\publish\%VERSION%
set OUTFILE=%OUTFOLDER%\naps2-%VERSION%-setup.msi
set SETUPBINFOLDER=..\..\NAPS2.Setup\bin\Release
mkdir %OUTFOLDER%
copy /Y %SETUPBINFOLDER%\NAPS2.Setup.msi %OUTFILE%
