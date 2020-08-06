echo f|xcopy ..\s\publish\BatchJobsApi\appsettings.release.json Artifacts\BatchJobsApi\appsettings.json /Y
echo f|xcopy ..\s\publish\ContentApi\appsettings.release.json Artifacts\ContentApi\appsettings.json /Y
echo f|xcopy ..\s\publish\ICCBackend\appsettings.release.json Artifacts\ICCBackend\appsettings.json /Y
echo f|xcopy ..\s\publish\MobileAppApi\appsettings.release.json Artifacts\MobileAppApi\appsettings.json /Y

REM double check later
echo f|xcopy ..\s\EksEngine\appsettings.release.json Artifacts\EksEngine\appsettings.json /Y
REM double check later
echo f|xcopy ..\s\ManifestEngine\appsettings.release.json Artifacts\ManifestEngine\appsettings.json /Y

echo f|xcopy ..\s\DbProvision\appsettings.release.json Artifacts\DbProvision\appsettings.json /Y
echo f|xcopy ..\s\DbFillExampleContent\appsettings.release.json Artifacts\DbFillExampleContent\appsettings.json /Y