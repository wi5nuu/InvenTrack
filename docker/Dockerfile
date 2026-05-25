FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/InvenTrack.Domain/InvenTrack.Domain.csproj", "src/InvenTrack.Domain/"]
COPY ["src/InvenTrack.Shared/InvenTrack.Shared.csproj", "src/InvenTrack.Shared/"]
COPY ["src/InvenTrack.Application/InvenTrack.Application.csproj", "src/InvenTrack.Application/"]
COPY ["src/InvenTrack.Infrastructure/InvenTrack.Infrastructure.csproj", "src/InvenTrack.Infrastructure/"]
COPY ["src/InvenTrack.API/InvenTrack.API.csproj", "src/InvenTrack.API/"]
COPY ["src/InvenTrack.Web/InvenTrack.Web.csproj", "src/InvenTrack.Web/"]

RUN dotnet restore "src/InvenTrack.API/InvenTrack.API.csproj"
RUN dotnet restore "src/InvenTrack.Web/InvenTrack.Web.csproj"

COPY . .
RUN dotnet build "src/InvenTrack.API/InvenTrack.API.csproj" -c Release -o /app/build
RUN dotnet build "src/InvenTrack.Web/InvenTrack.Web.csproj" -c Release -o /app/build
RUN dotnet publish "src/InvenTrack.API/InvenTrack.API.csproj" -c Release -o /app/publish/api
RUN dotnet publish "src/InvenTrack.Web/InvenTrack.Web.csproj" -c Release -o /app/publish/web

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=build /app/publish/api .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "InvenTrack.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS web
WORKDIR /app
COPY --from=build /app/publish/web .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InvenTrack.Web.dll"]
