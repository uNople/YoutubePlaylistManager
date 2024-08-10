# Use the official Microsoft .NET 8 SDK image with Windows support
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

COPY . .

RUN dotnet restore YouTubePlaylistManager.sln

RUN dotnet build --configuration Release

# Run tests and collect code coverage
RUN dotnet test --configuration Release --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test_results.trx"

# Publish the output to a directory
RUN dotnet publish --configuration Release --output /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set entry point - todo, modify to package up instead
ENTRYPOINT ["dotnet", "YourApp.dll"]
