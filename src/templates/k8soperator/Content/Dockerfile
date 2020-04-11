#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["__ProjectName__.csproj", "src/__ProjectName__/"]
RUN dotnet restore "src/__ProjectName__/__ProjectName__.csproj"
COPY . .
RUN dotnet build "__ProjectName__.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "__ProjectName__.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "__ProjectName__.dll"]