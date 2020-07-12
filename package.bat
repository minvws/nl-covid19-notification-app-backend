@echo off

REM Create package directory. This is needed because CI server can only publish artifacts by directory
cd publish
mkdir packages
cd ..

REM Zip all of the packages
7z a -tzip publish\packages\Azure.CdnDataPurge.zip publish\Azure\CdnDataPurge"
7z a -tzip publish\packages\Azure.CdnDataReceiver2.zip publish\Azure\CdnDataReceiver2"
7z a -tzip publish\packages\Azure.CdnRegionSync.zip publish\Azure\CdnRegionSync"
7z a -tzip publish\packages\BZ.CdnDataApi.zip publish\BZ\CdnDataApi"
7z a -tzip publish\packages\BZ.EKSEngineApi.zip publish\BZ\EKSEngineApi"
7z a -tzip publish\packages\BZ.ICCBackend.zip publish\BZ\ICCBackend"
7z a -tzip publish\packages\BZ.KeyReleasePortalApi.zip publish\BZ\KeyReleasePortalApi"
7z a -tzip publish\packages\BZ.KeysLastWorkflowApi.zip publish\BZ\KeysLastWorkflowApi"
7z a -tzip publish\packages\BZ.WorkflowStateEngineApi.zip publish\BZ\WorkflowStateEngineApi"
7z a -tzip publish\packages\DMZ.CdnPusherEngine2.zip publish\DMZ\CdnPusherEngine2"
7z a -tzip publish\packages\DMZ.IccPortalAuthorizer.zip publish\DMZ\IccPortalAuthorizer"
7z a -tzip publish\packages\Tools.DatabaseProvisioningTool.zip publish\Tools\DatabaseProvisioningTool"
7z a -tzip publish\packages\Tools.ScheduledTaskEngine.zip publish\Tools\ScheduledTaskEngine"

@echo on

