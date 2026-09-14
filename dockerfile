FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Movies.API/*.csproj Movies.API/
COPY Movies.Domain/*.csproj Movies.Domain/
COPY Movies.Infrastructure/*.csproj Movies.Infrastructure/
RUN dotnet restore Movies.API/Movies.API.csproj

COPY . .
RUN dotnet publish Movies.API/Movies.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Movies.API.dll"]