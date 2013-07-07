@echo off
cd %~dp0
set VERSION=1.0b1
set OUTFOLDER=..\publish\%VERSION%
set TEMPFOLDER=%OUTFOLDER%\naps2-%VERSION%-standalone
set OUTFILE=..\publish\%VERSION%\naps2-%VERSION%-standalone.zip
set BINFOLDER=..\bin\Standalone
mkdir %OUTFOLDER%
mkdir %TEMPFOLDER%
copy %BINFOLDER%\NAPS2.exe %TEMPFOLDER%
copy %BINFOLDER%\PdfSharp.dll %TEMPFOLDER%
copy %BINFOLDER%\Interop.WIA.dll %TEMPFOLDER%
copy %BINFOLDER%\Ninject.dll %TEMPFOLDER%
copy ..\Resources\scanner-app.ico %TEMPFOLDER%
copy lib\wiaaut.dll %TEMPFOLDER%
del %OUTFILE%
7z a %OUTFILE% %TEMPFOLDER%\*
rmdir /S /Q %TEMPFOLDER%
