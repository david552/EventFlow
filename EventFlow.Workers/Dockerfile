FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EventFlow.Workers/EventFlow.Workers.csproj", "EventFlow.Workers/"]
COPY ["EventFlow.Application/EventFlow.Application.csproj", "EventFlow.Application/"]
COPY ["EventFlow.Domain/EventFlow.Domain.csproj", "EventFlow.Domain/"]
COPY ["EventFlow.Persistence/EventFlow.Persistence.csproj", "EventFlow.Persistence/"]
COPY ["EventFlow.Infrastructure/EventFlow.Infrastructure.csproj", "EventFlow.Infrastructure/"]

RUN dotnet restore "EventFlow.Workers/EventFlow.Workers.csproj"

COPY . .
WORKDIR "/src/EventFlow.Workers"
RUN dotnet build "EventFlow.Workers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventFlow.Workers.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventFlow.Workers.dll"]