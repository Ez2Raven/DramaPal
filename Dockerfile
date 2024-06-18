FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7204
EXPOSE 5065

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/chatapp/", "chatapp/"]
COPY ["src/shared/", "shared/"]
RUN dotnet restore "chatapp/chatapp.csproj"
WORKDIR "/src/chatapp"
RUN dotnet build "chatapp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "chatapp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet dev-certs https --trust
ENTRYPOINT ["dotnet", "chatapp.dll"]
