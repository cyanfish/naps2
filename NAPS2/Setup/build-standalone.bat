@echo off
set VERSION=1.0b1
set OUTFOLDER=..\publish\naps2-%VERSION%-standalone
set OUTFILE=..\publish\naps2-%VERSION%-standalone.zip
set BINFOLDER=..\bin\Standalone
mkdir %OUTFOLDER%
copy %BINFOLDER%\NAPS2.exe %OUTFOLDER%
copy %BINFOLDER%\PdfSharp.dll %OUTFOLDER%
copy %BINFOLDER%\Interop.WIA.dll %OUTFOLDER%
copy %BINFOLDER%\Ninject.dll %OUTFOLDER%
copy ..\Resources\scanner-app.ico %OUTFOLDER%
copy lib\wiaaut.dll %OUTFOLDER%
del %OUTFILE%
7z a %OUTFILE% %OUTFOLDER%\*
rmdir /S /Q %OUTFOLDER%