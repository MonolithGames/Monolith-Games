# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY web/Monolith.Web/Monolith.Web.csproj web/Monolith.Web/
RUN dotnet restore web/Monolith.Web/Monolith.Web.csproj
COPY . .
RUN dotnet publish web/Monolith.Web/Monolith.Web.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:65000 \
    ASPNETCORE_ENVIRONMENT=Production \
    MONOLITH_DATA_PATH=/data
RUN mkdir -p /data && chown app:app /data
VOLUME ["/data"]
EXPOSE 65000
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Monolith.dll"]
