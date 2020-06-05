FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

ADD Components /app/Components
ADD ServerStandAlone /app/ServerStandAlone

# TODO: Copy appsettings to correct DB Migration Project

ADD docker/appsettings.Development.json /app/ServerStandAlone/appsettings.Development.json
ADD docker/appsettings.Development.json /app/Components/appsettings.Development.json
ADD docker/start.sh /app/start.sh
WORKDIR /app
ENTRYPOINT ["./start.sh"]
