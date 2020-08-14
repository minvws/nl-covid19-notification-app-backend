FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

COPY . app/
WORKDIR app/
COPY docker/standalone/appsettings.json .
RUN rm Components/Framework/WindowsIdentityStuff.cs
COPY docker/standalone/hacks/WindowsIdentityStuff.cs Components/Framework/

# get rid of xcopy commands in project files
RUN find . -name \*.csproj -exec sed -i '/<Exec Command="xcopy.*>/d' {} \;
RUN dotnet publish BatchJobsApi/BatchJobsApi.csproj --no-self-contained --configuration Release -o publish/BatchJobsApi --version-suffix local
RUN dotnet publish ContentApi/ContentApi.csproj --no-self-contained  --configuration Release -o publish/ContentApi --version-suffix local
#RUN dotnet publish ICCBackend/ICCBackend.csproj --no-self-contained  --configuration Release -o publish/ICCBackend --version-suffix local
RUN dotnet publish MobileAppApi/MobileAppApi.csproj --no-self-contained  --configuration Release -o publish/MobileAppApi --version-suffix local
#RUN dotnet publish ManagementPortal/ManagementPortal.csproj --no-self-contained  --configuration Release -o publish/ManagementPortal --version-suffix local

RUN dotnet publish EksEngine/EksEngine.csproj --no-self-contained  --configuration Release -o publish/EksEngine --version-suffix local
RUN dotnet publish ManifestEngine/ManifestEngine.csproj --no-self-contained  --configuration Release -o publish/ManifestEngine --version-suffix local

RUN dotnet publish DbProvision/DbProvision.csproj --no-self-contained  --configuration Release -o publish/Tools/DbProvision --version-suffix local
RUN dotnet publish DbFillExampleContent/DbFillExampleContent.csproj --no-self-contained  --configuration Release -o publish/Tools/DbFillExampleContent --version-suffix local
RUN dotnet publish GenTeks/GenTeks.csproj --no-self-contained  --configuration Release -o publish/Tools/GenTeks --version-suffix local
RUN dotnet publish ForceTekAuth/ForceTekAuth.csproj --no-self-contained  --configuration Release -o publish/Tools/ForceTekAuth --version-suffix local

RUN find ./publish -name appsettings.json -exec cp appsettings.json {} \;
COPY docker/standalone/config/appconfig.json publish/Tools/DbFillExampleContent/
COPY docker/standalone/config/riskparams.json publish/Tools/DbFillExampleContent/

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
WORKDIR /app
COPY --from=builder app/publish/ .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="http://*:5000;https://*:5001"
