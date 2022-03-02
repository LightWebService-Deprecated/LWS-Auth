FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LWSAuthService/LWSAuthService.csproj", "LWSAuthService/"]
RUN dotnet restore "LWSAuthService/LWSAuthService.csproj"
COPY . .
WORKDIR "/src/LWSAuthService"
RUN dotnet build "LWSAuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LWSAuthService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LWSAuthService.dll"]
