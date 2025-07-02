FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY Front/Front.csproj Front/
COPY Models/Models.csproj Models/

RUN dotnet restore Front/Front.csproj

COPY . .

WORKDIR /src/Front

RUN dotnet publish Front.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Front.dll"]
