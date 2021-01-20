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
REM 7z a -tzip packages\App\Website.IccBackend.zip publish\IccBackend"
REM 7z a -tzip packages\App\Website.MobileAppApi.zip publish\MobileAppApi"
REM xcopy ..\s\src\publish\BatchJobsApi Artifacts\BatchJobsApi /E /I

xcopy ..\s\src\publish\ContentApi Artifacts\ContentApi /E /I
xcopy ..\s\src\publish\IccBackend Artifacts\IccBackend /E /I
xcopy ..\s\src\publish\MobileAppApi Artifacts\MobileAppApi /E /I
xcopy ..\s\src\publish\ManagementPortal Artifacts\ManagementPortal /E /I

REM Zip command-line
REM 7z a -tzip packages\Batch\CommandLine.EksEngine.zip publish\Batch.EksEngine"
REM 7z a -tzip packages\Batch\CommandLine.ManifestEngine.zip publish\Batch.ManifestEngine"

xcopy ..\s\src\publish\EksEngine Artifacts\EksEngine /E /I
xcopy ..\s\src\publish\ManifestEngine Artifacts\ManifestEngine /E /I
xcopy ..\s\src\publish\DailyCleanup Artifacts\DailyCleanup /E /I
xcopy ..\s\src\publish\EfgsDownloader Artifacts\EfgsDownloader /E /I
xcopy ..\s\src\publish\EfgsUploader Artifacts\EfgsUploader /E /I

REM Database
REM 7z a -tzip packages\SQL\SQL.DbProvision.zip publish\Tools\DbProvision"

xcopy ..\s\src\publish\Tools\PublishContent Artifacts\PublishContent /E /I
xcopy ..\s\src\publish\Tools\SigTestFileCreator Artifacts\SigTestFileCreator /E /I

xcopy ..\s\src\HSM-Scripting Artifacts\HSM-Scripting /E /I

REM Zip tools
REM 7z a -tzip packages\Tools\CommandLine.GenTeks.zip publish\Tools\GenTeks"

@echo on
