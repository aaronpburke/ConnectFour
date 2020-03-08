#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

# Copy the project files and restore their Nuget packages prior to build to maximize Docker cache efficiency
# and minimize build time.

COPY NuGet.Config .

COPY ConnectFour.Api/ConnectFour.Api.csproj ConnectFour.Api/
COPY ConnectFour.ServiceLayer/ConnectFour.ServiceLayer.csproj ConnectFour.ServiceLayer/
COPY ConnectFour.DataLayer/ConnectFour.DataLayer.csproj ConnectFour.DataLayer/
RUN dotnet restore ConnectFour.Api/ConnectFour.Api.csproj

# Copy the rest of the source
COPY . .

WORKDIR "/src/ConnectFour.Api"
RUN dotnet build "ConnectFour.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectFour.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConnectFour.Api.dll"]