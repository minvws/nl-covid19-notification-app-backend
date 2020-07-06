@echo off

REM Zip all of the packages
7z a -tzip publish\Azure.CdnApi.zip publish\Azure\CdnApi
7z a -tzip publish\Azure.CdnApi.zip publish\Azure\CdnDataPurge"
7z a -tzip publish\Azure.CdnApi.zip publish\Azure\CdnDataReceiver"
7z a -tzip publish\Azure.CdnApi.zip publish\Azure\CdnDataReceiver2"
7z a -tzip publish\Azure.CdnApi.zip publish\Azure\CdnRegionSync"
7z a -tzip publish\BZ.KeysLastWorkflowApi.zip publish\BZ\KeysLastWorkflowApi"
7z a -tzip publish\BZ.CdnDataApi.zip publish\BZ\CdnDataApi"
7z a -tzip publish\BZ.EKSEngineApi.zip publish\BZ\EKSEngineApi"
7z a -tzip publish\BZ.ICCBackend.zip publish\BZ\ICCBackend"
7z a -tzip publish\BZ.KeyReleasePortalApi.zip publish\BZ\KeyReleasePortalApi"
7z a -tzip publish\BZ.WorkflowStateEngineApi.zip publish\BZ\WorkflowStateEngineApi"
7z a -tzip publish\DMZ.ContentPusherEngine.zip publish\DMZ\ContentPusherEngine"
7z a -tzip publish\DMZ.CdnPusherEngine2.zip publish\DMZ\CdnPusherEngine2"
7z a -tzip publish\DMZ.ICCPortal.zip publish\DMZ\ICCPortal"
7z a -tzip publish\Tools.DatabaseProvisioningTool.zip publish\Tools\DatabaseProvisioningTool"
7z a -tzip publish\Tools.ScheduledTaskEngine.zip publish\Tools\ScheduledTaskEngine"

@echo on

