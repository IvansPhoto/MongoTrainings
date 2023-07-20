FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MongoTrainings.Aggregatoin/MongoTrainings.Aggregatoin.csproj", "MongoTrainings.Aggregatoin/"]
RUN dotnet restore "MongoTrainings.Aggregatoin/MongoTrainings.Aggregatoin.csproj"
COPY . .
WORKDIR "/src/MongoTrainings.Aggregatoin"
RUN dotnet build "MongoTrainings.Aggregatoin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MongoTrainings.Aggregatoin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MongoTrainings.Aggregatoin.dll"]
