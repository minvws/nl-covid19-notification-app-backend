@echo off

REM Create package directory. This is needed because CI server can only publish artifacts by directory
mkdir packages

REM Remove any configuration files which have sneaked in to the packages
cd publish
del /S *.config
del /S appsettings.json
cd ..

REM Zip websites
7z a -tzip packages\Batch\Website.BatchJobsApi.zip publish\BatchJobsApi"
7z a -tzip packages\App\Website.ContentApi.zip publish\ContentApi"
7z a -tzip packages\App\Website.ICCBackend.zip publish\ICCBackend"
7z a -tzip packages\App\Website.MobileAppApi.zip publish\MobileAppApi"

REM Zip command-line
7z a -tzip packages\Batch\CommandLine.EksEngine.zip publish\EksEngine"
7z a -tzip packages\Batch\CommandLine.ManifestEngine.zip publish\ManifestEngine"

REM Database
7z a -tzip packages\SQL\SQL.DbProvision.zip publish\Tools\DbProvision"

REM Zip tools
7z a -tzip packages\Tools\CommandLine.GenTeks.zip publish\Tools\GenTeks"
7z a -tzip packages\Tools\CommandLine.DbFillExampleContent.zip publish\Tools\DbFillExampleContent"
7z a -tzip packages\Tools\CommandLine.ForceTekAuth.zip publish\Tools\ForceTekAuth"

@echo on
