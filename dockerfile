FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /
COPY ["Service/Service.csproj", "Service/"]
RUN dotnet restore "Service/Service.csproj"
COPY . .
WORKDIR /
RUN dotnet build "Service/Service.csproj" -c $BUILD_CONFIGURATION -o /build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Service/Service.csproj" -c $BUILD_CONFIGURATION -o /publish /p:UseAppHost=false

FROM base AS final
WORKDIR /
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]