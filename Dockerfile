# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY SudoBot/*.csproj ./

# Add DSharp Package Source
RUN dotnet nuget add source https://nuget.emzi0767.com/api/v3/index.json --name DSharp


RUN dotnet restore

COPY SudoBot ./
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as run-env
WORKDIR /app

COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SudoBot.dll"]