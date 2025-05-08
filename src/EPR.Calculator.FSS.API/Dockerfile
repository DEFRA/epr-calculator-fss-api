#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["EPR.Calculator.FSS.API/EPR.Calculator.FSS.API.csproj", "EPR.Calculator.FSS.API/"]
COPY EPR.Calculator.FSS.API.Common/. ./EPR.Calculator.FSS.API.Common/.

# Copy the nuget.config and local package file
COPY EPR.Calculator.FSS.API/Package/NuGet.Config ./NuGet.Config
#COPY EPR.Calculator.FSS.API/Package ./Package

# Restore dependencies
RUN dotnet restore "./EPR.Calculator.FSS.API.csproj" /p:AzureBuild=true --configfile .NuGet.Config

# Copy the rest of the source code
COPY . .

# Set the working directory and build the project
WORKDIR "/src/EPR.Calculator.FSS.API"
RUN dotnet build "./EPR.Calculator.FSS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Calculator.FSS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Calculator.FSS.API.dll"]