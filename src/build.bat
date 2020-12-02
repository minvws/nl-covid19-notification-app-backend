@echo off

REM Version suffix starts with build number, defaults to 0 so local/ad-hock builds are clearly marked
set suffix=%1
if "%suffix%"=="" set suffix=0

REM Append the current git hash to the version suffix
FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --short HEAD`) DO (
  SET suffix= "%suffix%-%%F"  
)

REM Publish websites
dotnet publish ContentApi\ContentApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\ContentApi --version-suffix %suffix%
dotnet publish IccBackend\IccBackend.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\IccBackend --version-suffix %suffix%
dotnet publish MobileAppApi\MobileAppApi.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\MobileAppApi --version-suffix %suffix%

REM Publish command-line
dotnet publish EksEngine\EksEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EksEngine --version-suffix %suffix%
dotnet publish ManifestEngine\ManifestEngine.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\ManifestEngine --version-suffix %suffix%
dotnet publish DailyCleanup\DailyCleanup.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\DailyCleanup --version-suffix %suffix%
dotnet publish EfgsDownloader\EfgsDownloader.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EfgsDownloader --version-suffix %suffix%
dotnet publish EfgsUploader\EfgsUploader.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\EfgsUploader --version-suffix %suffix%

REM Publish tools
dotnet publish DbProvision\DbProvision.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\DbProvision --version-suffix %suffix%
dotnet publish PublishContent\PublishContent.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\PublishContent --version-suffix %suffix%
dotnet publish GenTeks\GenTeks.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\GenTeks --version-suffix %suffix%
dotnet publish ForceTekAuth\ForceTekAuth.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\ForceTekAuth --version-suffix %suffix%
dotnet publish SigTestFileCreator\SigTestFileCreator.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\SigTestFileCreator
dotnet publish EksParser\EksParser.csproj --no-self-contained --runtime win-x64 --configuration Release -o publish\Tools\EksParser --version-suffix %suffix%

@echo on

IF %ERRORLEVEL% NEQ 0 EXIT 1
