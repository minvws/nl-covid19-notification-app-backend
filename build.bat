@echo off

REM Version suffix starts with build number, defaults to 0 so local/ad-hock builds are clearly marked
set suffix=%1
if "%suffix%"=="" set suffix=0

REM Append the current git hash to the version suffix
FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --short HEAD`) DO (
  SET suffix= "%suffix%-%%F"  
)

REM Publish all of the deployable projects
dotnet publish CdnApi\CdnApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Azure\CdnApi --version-suffix %suffix%
dotnet publish CdnDataPurge\CdnDataPurge.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Azure\CdnDataPurge --version-suffix %suffix%
dotnet publish CdnDataReceiver\CdnDataReceiver.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Azure\CdnDataReceiver --version-suffix %suffix%
dotnet publish CdnDataReceiver2\CdnDataReceiver2.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Azure\CdnDataReceiver2 --version-suffix %suffix%
dotnet publish CdnRegionSync\CdnRegionSync.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Azure\CdnRegionSync --version-suffix %suffix%
dotnet publish KeysLastWorkflowApi\WorkflowApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\KeysLastWorkflowApi --version-suffix %suffix%
dotnet publish CdnDataApi\CdnDataApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\CdnDataApi --version-suffix %suffix%
dotnet publish EKSEngineApi\EKSEngineApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\EKSEngineApi --version-suffix %suffix%
dotnet publish ICCBackend\ICCBackend.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\ICCBackend --version-suffix %suffix%
dotnet publish KeyReleasePortalApi\KeyReleaseApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\KeyReleasePortalApi --version-suffix %suffix%
dotnet publish WorkflowStateEngineApi\WorkflowStateEngineApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\BZ\WorkflowStateEngineApi --version-suffix %suffix%
dotnet publish ContentPusherEngine\CdnPusherEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\DMZ\ContentPusherEngine --version-suffix %suffix%
dotnet publish CdnPusherEngine2\CdnPusherEngine2.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\DMZ\CdnPusherEngine2 --version-suffix %suffix%
dotnet publish ICCPortal\IccPortalAuthorizer.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\DMZ\IccPortalAuthorizer --version-suffix %suffix%
dotnet publish DatabaseProvisioningTool\DatabaseProvisioningTool.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\DatabaseProvisioningTool --version-suffix %suffix%
dotnet publish ScheduledTaskEngine\ScheduledTaskEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\ScheduledTaskEngine --version-suffix %suffix%

@echo on

