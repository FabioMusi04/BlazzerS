# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Front/Front.csproj Front/
COPY Models/Models.csproj Models/
RUN dotnet restore Front/Front.csproj

COPY . .
WORKDIR /src/Front
RUN dotnet publish Front.csproj -c Release -o /app/publish

# Final image
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy Blazor WASM published files
COPY --from=build /app/publish/wwwroot . 

# Optional: custom nginx config for SPA routing
COPY Front/wwwroot/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
