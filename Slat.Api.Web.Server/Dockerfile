#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Slat.Api.Web.Server/Slat.Api.Web.Server.csproj", "Slat.Api.Web.Server/"]
RUN dotnet restore "Slat.Api.Web.Server/Slat.Api.Web.Server.csproj"
COPY . .
WORKDIR "/src/Slat.Api.Web.Server"
RUN dotnet build "Slat.Api.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Slat.Api.Web.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slat.Api.Web.Server.dll"]