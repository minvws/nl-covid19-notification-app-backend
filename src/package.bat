@echo off

REM Create package directory. This is needed because CI server can only publish artifacts by directory
mkdir packages

REM Remove any configuration files which have sneaked in to the packages
cd publish
del /S *.config
del /S appsettings.json
cd ..

REM Zip websites
7z a -tzip packages\App\Website.ContentApi.zip publish\ContentApi"
7z a -tzip packages\App\Website.IccBackend.zip publish\IccBackend"
7z a -tzip packages\App\Website.MobileAppApi.zip publish\MobileAppApi"

REM Zip command-line
7z a -tzip packages\Batch\CommandLine.EksEngine.zip publish\EksEngine"
7z a -tzip packages\Batch\CommandLine.ManifestEngine.zip publish\ManifestEngine"
7z a -tzip packages\Batch\CommandLine.DailyCleanup.zip publish\DailyCleanup"
7z a -tzip packages\Batch\CommandLine.EfgsDownloader.zip publish\EfgsDownloader"
7z a -tzip packages\Batch\CommandLine.EfgsUploader.zip publish\EfgsUploader"

REM Database
7z a -tzip packages\SQL\SQL.DbProvision.zip publish\Tools\DbProvision"

REM Zip tools
7z a -tzip packages\Tools\CommandLine.GenTeks.zip publish\Tools\GenTeks"
7z a -tzip packages\Tools\CommandLine.PublishContent.zip publish\Tools\PublishContent"
7z a -tzip packages\Tools\CommandLine.ForceTekAuth.zip publish\Tools\ForceTekAuth"
7z a -tzip packages\Tools\CommandLine.SigTestFileCreator.zip publish\Tools\SigTestFileCreator"
7z a -tzip packages\Tools\CommandLine.EksParser.zip publish\Tools\EksParser"

@echo on
