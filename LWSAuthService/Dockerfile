FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LWSAuthService/LWSAuthService.csproj", "LWSAuthService/"]
RUN dotnet restore "LWSAuthService/LWSAuthService.csproj"
COPY . .
WORKDIR "/src/LWSAuthService"
RUN dotnet build "LWSAuthService.csproj" -c Debug -o /app/build
RUN dotnet publish "LWSAuthService.csproj" -c Debug -o /app/publish
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "LWSAuthService.dll"]
