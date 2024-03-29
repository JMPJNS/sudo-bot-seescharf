# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY SudoBot/*.csproj ./

RUN dotnet restore

COPY SudoBot ./
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as run-env
WORKDIR /app

# Install Deps
RUN apt-get update -y && apt-get install ffmpeg youtube-dl python3-distutils python3-apt curl -y

RUN curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py
RUN python3 get-pip.py
RUN pip install --upgrade youtube_dl


COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SudoBot.dll"]
