#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["TodoList.Api/TodoList.Api.csproj", "TodoList.Api/"]
COPY ["TodoList.Data/TodoList.Data.csproj", "TodoList.Data/"]
COPY ["TodoList.Business/TodoList.Business.csproj", "TodoList.Business/"]
RUN dotnet restore "TodoList.Api/TodoList.Api.csproj"
COPY . .
WORKDIR "/src/TodoList.Api"
RUN dotnet build "TodoList.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.Api.dll"]