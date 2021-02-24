@echo off

REM Version suffix starts with build number, defaults to 0 so local/ad-hock builds are clearly marked
set suffix=%1
if "%suffix%"=="" set suffix=0

REM Append the current git hash to the version suffix
FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --short HEAD`) DO (
   SET suffix= "%suffix%-%%F"  
)

REM Publish websites
dotnet publish Content.WebApi\Content.WebApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\ContentApi --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish Icc.WebApi\Icc.WebApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\IccBackend --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish MobileAppApi.WebApi\MobileAppApi.WebApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\MobileAppApi --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish ManagementPortal\ManagementPortal.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\ManagementPortal --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1

REM Publish command-line
dotnet publish EksEngine\EksEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EksEngine --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish ManifestEngine\ManifestEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\ManifestEngine --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish DailyCleanup\DailyCleanup.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\DailyCleanup --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish Iks.Downloader\Iks.Downloader.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EfgsDownloader --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish Iks.Uploader\Iks.Uploader.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EfgsUploader --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1

REM Publish tools
dotnet publish PublishContent\PublishContent.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\PublishContent --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish GenTeks\GenTeks.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\GenTeks --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish ForceTekAuth\ForceTekAuth.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\ForceTekAuth --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish SigTestFileCreator\SigTestFileCreator.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\SigTestFileCreator
IF %ERRORLEVEL% NEQ 0 EXIT 1
dotnet publish Eks.Parser\Eks.Parser.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\EksParser --version-suffix %suffix%
IF %ERRORLEVEL% NEQ 0 EXIT 1

@echo on
