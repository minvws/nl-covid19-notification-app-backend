@echo off

cd publish

REM Remove any confifugration files which have sneaked in to the packages
del /S *.config
del /S appsettings.json

REM Create package directory. This is needed because CI server can only publish artifacts by directory
mkdir packages
cd ..

REM Zip websites
7z a -tzip publish\packages\BatchJobsApi.zip publish\BatchJobsApi"
7z a -tzip publish\packages\ContentApi.zip publish\ContentApi"
7z a -tzip publish\packages\ICCBackend.zip publish\ICCBackend"
7z a -tzip publish\packages\MobileAppApi.zip publish\MobileAppApi"

REM Zip command-line
7z a -tzip publish\packages\EksEngine.zip publish\EksEngine"
7z a -tzip publish\packages\ManifestEngine.zip publish\ManifestEngine"

REM Zip tools
7z a -tzip publish\packages\Tools\DbProvision.zip publish\Tools\DbProvision"
7z a -tzip publish\packages\Tools\GenTeks.zip publish\Tools\GenTeks"

REM Zip stand-alone server
REM 7z a -tzip publish\packages\ServerStandAlone.zip publish\ServerStandAlone"

@echo on
