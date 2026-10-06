FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/IronAndIvoryCo
RUN dotnet restore IronAndIvoryCo.csproj
RUN dotnet publish IronAndIvoryCo.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "IronAndIvoryCo.dll"]
