@echo off

REM Create package directory. This is needed because CI server can only publish artifacts by directory
REM mkdir packages

REM Remove any configuration files which have sneaked in to the packages
REM cd publish
REM del /S *.config
REM del /S appsettings.json
REM cd ..

REM --notice: at CIBG we don't use ZIP files for artifacts
REM Zip websites
REM 7z a -tzip packages\Batch\Website.BatchJobsApi.zip publish\BatchJobsApi"
REM 7z a -tzip packages\App\Website.ContentApi.zip publish\ContentApi"
REM 7z a -tzip packages\App\Website.ICCBackend.zip publish\ICCBackend"
REM 7z a -tzip packages\App\Website.MobileAppApi.zip publish\MobileAppApi"

xcopy ..\s\publish\BatchJobsApi Artifacts\BatchJobsApi /E /I
xcopy ..\s\publish\ContentApi Artifacts\ContentApi /E /I
xcopy ..\s\publish\ICCBackend Artifacts\ICCBackend /E /I
xcopy ..\s\publish\MobileAppApi Artifacts\MobileAppApi /E /I
xcopy ..\s\publish\ManagementPortal Artifacts\ManagementPortal /E /I

REM Zip command-line
REM 7z a -tzip packages\Batch\CommandLine.EksEngine.zip publish\Batch.EksEngine"
REM 7z a -tzip packages\Batch\CommandLine.ManifestEngine.zip publish\Batch.ManifestEngine"

xcopy ..\s\publish\EksEngine Artifacts\EksEngine /E /I
xcopy ..\s\publish\ManifestEngine Artifacts\ManifestEngine /E /I

REM Database
REM 7z a -tzip packages\SQL\SQL.DbProvision.zip publish\Tools\DbProvision"

xcopy ..\s\publish\Tools\DbProvision Artifacts\DbProvision /E /I
xcopy ..\s\publish\Tools\DbFillExampleContent Artifacts\DbFillExampleContent /E /I
xcopy ..\s\publish\Tools\GenTeks Artifacts\GenTeks /E /I
xcopy ..\s\publish\Tools\ForceTekAuth Artifacts\ForceTekAuth /E /I
xcopy ..\s\publish\Tools\SigTestFileCreator Artifacts\SigTestFileCreator /E /I
xcopy ..\s\publish\Tools\EksParser Artifacts\EksParser /E /I

REM Zip tools
REM 7z a -tzip packages\Tools\CommandLine.GenTeks.zip publish\Tools\GenTeks"

@echo on
