FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Front/Front.csproj Front/
COPY Models/Models.csproj Models/

RUN dotnet restore Front/Front.csproj

COPY . ./

RUN dotnet publish Front/Front.csproj -c Release -o /app/publish
