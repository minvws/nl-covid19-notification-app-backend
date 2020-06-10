FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

ADD . /app/TestPortal
ADD appsettings.Development.json /app/TestPortal/appsettings.json

WORKDIR /app/TestPortal
ENTRYPOINT ["dotnet", "run"]