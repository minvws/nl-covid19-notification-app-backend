echo This is the first version of the script, it creates websites of the published files

SET CurrentDir=%~dp0
ECHO Current directory is:
ECHO %CurrentDir%

REM Create web sites (with app pools & correct defaults)
ECHO Creating websites
%systemroot%\system32\inetsrv\AppCmd.exe add site /name:C19_BatchJobsApi /bindings:"https/*.7000" /physicalPath:"%CurrentDir%\publish\BatchJobsApi"
%systemroot%\system32\inetsrv\AppCmd.exe add site /name:C19_ContentApi /bindings:"https/*.7001" /physicalPath:"%CurrentDir%\publish\ContentApi"
%systemroot%\system32\inetsrv\AppCmd.exe add site /name:C19_MobileAppApi /bindings:"https/*.7002" /physicalPath:"%CurrentDir%\publish\MobileAppApi"
%systemroot%\system32\inetsrv\AppCmd.exe add site /name:C19_IccBackend /bindings:"https/*.7002" /physicalPath:"%CurrentDir%\publish\IccBackend"
