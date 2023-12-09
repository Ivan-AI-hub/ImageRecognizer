#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

EXPOSE 5100

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImageRecognizer.DistributionServer/ImageRecognizer.DistributionServer.csproj", "ImageRecognizer.DistributionServer/"]
COPY ["ImageRecognizer.DistributionServer/wwwroot", "/app/wwwroot/"]
COPY ["ImageRecognizer.Domain/ImageRecognizer.Domain.csproj", "ImageRecognizer.Domain/"]

RUN dotnet restore "./ImageRecognizer.DistributionServer/./ImageRecognizer.DistributionServer.csproj"
COPY . .
WORKDIR "/src/ImageRecognizer.DistributionServer"
RUN dotnet build "./ImageRecognizer.DistributionServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageRecognizer.DistributionServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageRecognizer.DistributionServer.dll"]