FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app
COPY . .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

RUN dotnet restore
RUN dotnet publish -o /app/published-app
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/published-app /app

ENTRYPOINT ["dotnet", "/app/TestService.dll"]
