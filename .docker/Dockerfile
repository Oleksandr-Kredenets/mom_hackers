# Build (from repository root):
#   docker build -f .docker/Dockerfile . -t hackathon-back

# Run:
#   docker run -p 8080:8080 hackathon-back

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/Web/Web.csproj backend/Web/
RUN dotnet restore backend/Web/Web.csproj

COPY backend/ ./backend/
RUN dotnet publish backend/Web/Web.csproj -c Release -o /app/publish --no-restore


# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV SERVER_PORT=8080
ENV ASPNETCORE_URLS=http://+:$SERVER_PORT
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE $SERVER_PORT

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]