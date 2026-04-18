FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EventFlow.MVC/EventFlow.MVC.csproj", "EventFlow.MVC/"]
COPY ["EventFlow.Application/EventFlow.Application.csproj", "EventFlow.Application/"]
COPY ["EventFlow.Domain/EventFlow.Domain.csproj", "EventFlow.Domain/"]
COPY ["EventFlow.Infrastructure/EventFlow.Infrastructure.csproj", "EventFlow.Infrastructure/"]
COPY ["EventFlow.Persistence/EventFlow.Persistence.csproj", "EventFlow.Persistence/"]

RUN dotnet restore "EventFlow.MVC/EventFlow.MVC.csproj"

COPY . .
WORKDIR "/src/EventFlow.MVC"
RUN dotnet build "EventFlow.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventFlow.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventFlow.MVC.dll"]