FROM defradigital/dotnetcore-development:dotnet8.0 AS base
 
USER root
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}
 
RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
 
FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root

# Install dotnet-ef tool
RUN dotnet tool install dotnet-ef --version 8.* --tool-path "/root/.dotnet/tools"

# Add the tools folder to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src
COPY ["EPR.Calculator.FSS.API/EPR.Calculator.FSS.API.csproj", "EPR.Calculator.FSS.API/"]
COPY EPR.Calculator.FSS.API.Common/. ./EPR.Calculator.FSS.API.Common/.

# Copy the nuget.config and local package file
COPY EPR.Calculator.FSS.API/NuGet.Config ./NuGet.Config
COPY EPR.Calculator.FSS.API/Package ./EPR.Calculator.FSS.API/Package


# Debugging steps
RUN ls -la /src
RUN ls -la /src/EPR.Calculator.FSS.API
RUN ls -la /src/EPR.Calculator.FSS.API/Package
RUN cat /src/NuGet.Config

RUN dotnet restore "./EPR.Calculator.FSS.API/EPR.Calculator.FSS.API.csproj" --configfile ./NuGet.Config

# Copy the rest of the source code
COPY --chown=dotnet:dotnet EPR.Calculator.FSS.API/. ./EPR.Calculator.FSS.API/.

# Set the working directory and build the project
WORKDIR "/src/EPR.Calculator.FSS.API"
RUN dotnet build "./EPR.Calculator.FSS.API.csproj" -c Release /p:AzureBuild=true -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Calculator.FSS.API.csproj" -c Release /p:AzureBuild=false -o /app/publish

# Final runtime image
FROM base AS final
USER dotnet
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Calculator.FSS.API.dll"]