@echo off
cd %~dp0
set VERSION=2.3
set OUTFOLDER=..\publish\%VERSION%
set TEMPFOLDER=%OUTFOLDER%\naps2-%VERSION%-standalone
set OUTFILE=..\publish\%VERSION%\naps2-%VERSION%-standalone.zip
set BINFOLDER=..\bin\Standalone
set CMDBINFOLDER=..\..\NAPS2.Console\bin\Standalone
mkdir %OUTFOLDER%
mkdir %TEMPFOLDER%
mkdir %TEMPFOLDER%\es
mkdir %TEMPFOLDER%\ru
mkdir %TEMPFOLDER%\uk
copy %BINFOLDER%\de\NAPS2.resources.dll %TEMPFOLDER%\de
copy %BINFOLDER%\es\NAPS2.resources.dll %TEMPFOLDER%\es
copy %BINFOLDER%\it\NAPS2.resources.dll %TEMPFOLDER%\it
copy %BINFOLDER%\ru\NAPS2.resources.dll %TEMPFOLDER%\ru
copy %BINFOLDER%\uk\NAPS2.resources.dll %TEMPFOLDER%\uk
copy %BINFOLDER%\NAPS2.exe %TEMPFOLDER%
copy %BINFOLDER%\PdfSharp.dll %TEMPFOLDER%
copy %BINFOLDER%\Interop.WIA.dll %TEMPFOLDER%
copy %BINFOLDER%\Ninject.dll %TEMPFOLDER%
copy %BINFOLDER%\NLog.dll %TEMPFOLDER%
copy %CMDBINFOLDER%\NAPS2.Console.exe %TEMPFOLDER%
copy %CMDBINFOLDER%\CommandLine.dll %TEMPFOLDER%
copy ..\Resources\scanner-app.ico %TEMPFOLDER%
copy lib\wiaaut.dll %TEMPFOLDER%
del %OUTFILE%
7z a %OUTFILE% %TEMPFOLDER%\*
rmdir /S /Q %TEMPFOLDER%
