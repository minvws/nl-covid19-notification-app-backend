echo f|xcopy ..\s\src\publish\BatchJobsApi\appsettings.release.json Artifacts\BatchJobsApi\appsettings.json /Y
echo f|xcopy ..\s\src\publish\ContentApi\appsettings.release.json Artifacts\ContentApi\appsettings.json /Y
echo f|xcopy ..\s\src\publish\IccBackend\appsettings.release.json Artifacts\IccBackend\appsettings.json /Y
echo f|xcopy ..\s\src\publish\MobileAppApi\appsettings.release.json Artifacts\MobileAppApi\appsettings.json /Y
echo f|xcopy ..\s\src\publish\ManagementPortal\appsettings.release.json Artifacts\ManagementPortal\appsettings.json /Y

REM double check later
echo f|xcopy ..\s\src\EksEngine\appsettings.release.json Artifacts\EksEngine\appsettings.json /Y
echo f|xcopy ..\s\src\ManifestEngine\appsettings.release.json Artifacts\ManifestEngine\appsettings.json /Y
echo f|xcopy ..\s\src\DailyCleanup\appsettings.release.json Artifacts\DailyCleanup\appsettings.json /Y
echo f|xcopy ..\s\src\Iks.Downloader\appsettings.release.json Artifacts\EfgsDownloader\appsettings.json /Y
echo f|xcopy ..\s\src\Iks.Uploader\appsettings.release.json Artifacts\EfgsUploader\appsettings.json /Y

echo f|xcopy ..\s\src\PublishContent\appsettings.release.json Artifacts\PublishContent\appsettings.json /Y
echo f|xcopy ..\s\src\SigTestFileCreator\appsettings.release.json Artifacts\SigTestFileCreator\appsettings.json /Y