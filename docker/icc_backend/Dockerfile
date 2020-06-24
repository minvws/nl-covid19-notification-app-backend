FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

ADD Components /app/Components
ADD GaenFormat /app/GaenFormat
ADD ICCBackend /app/ICCBackend

ADD docker/icc_backend/appsettings.Development.json /app/ICCBackend/appsettings.json

ADD docker/icc_backend/start.sh /app/start.sh
WORKDIR /app
ENTRYPOINT ["sh", "start.sh"]