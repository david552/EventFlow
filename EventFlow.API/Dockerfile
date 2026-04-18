FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EventFlow.API/EventFlow.API.csproj", "EventFlow.API/"]
COPY ["EventFlow.Application/EventFlow.Application.csproj", "EventFlow.Application/"]
COPY ["EventFlow.Domain/EventFlow.Domain.csproj", "EventFlow.Domain/"]
COPY ["EventFlow.Infrastructure/EventFlow.Infrastructure.csproj", "EventFlow.Infrastructure/"]
COPY ["EventFlow.Persistence/EventFlow.Persistence.csproj", "EventFlow.Persistence/"]

RUN dotnet restore "EventFlow.API/EventFlow.API.csproj"

COPY . .
WORKDIR "/src/EventFlow.API"
RUN dotnet build "EventFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventFlow.API.dll"]