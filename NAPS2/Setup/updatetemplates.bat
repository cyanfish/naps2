cd ../../NAPS2.Core
mkdir temp
cp WinForms/*.resx temp/
cp Lang/Resources/*.resx temp/
rm temp/*.*.resx
pause
rm temp/*.resx
rmdir temp/
