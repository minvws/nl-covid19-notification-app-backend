echo f|xcopy ..\s\publish\appsettings.release.json Artifacts\ContentApi\appsettings.json /Y
echo f|xcopy ..\s\publish\appsettings.release.json Artifacts\ICCBackend\appsettings.json /Y
echo f|xcopy ..\s\publish\appsettings.release.json Artifacts\MobileAppApi\appsettings.json /Y

echo f|xcopy ..\s\appsettings.release.json Artifacts\EksEngine\appsettings.json /Y
echo f|xcopy ..\s\appsettings.release.json Artifacts\ManifestEngine\appsettings.json /Y
echo f|xcopy ..\s\appsettings.release.json Artifacts\DbProvision\appsettings.json /Y