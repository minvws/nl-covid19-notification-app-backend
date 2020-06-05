FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

ADD Components /app/Components
ADD ServerStandAlone /app/ServerStandAlone
ADD GaenFormat /app/GaenFormat

ADD docker/appsettings.Development.json /app/ServerStandAlone/appsettings.json

ADD docker/start.sh /app/start.sh
WORKDIR /app
ENTRYPOINT ["sh", "start.sh"]
