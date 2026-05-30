FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj", "Fundo.Applications.WebApi/"]
RUN dotnet restore "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"
COPY . .
WORKDIR "/src/Fundo.Applications.WebApi"
RUN dotnet build "Fundo.Applications.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fundo.Applications.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
