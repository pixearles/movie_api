FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Movies.Web/*.csproj Movies.Web/
COPY Movies.Web.Client/*.csproj Movies.Web.Client/
RUN dotnet restore Movies.Web/Movies.Web.csproj

COPY . .
RUN dotnet publish Movies.Web/Movies.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Movies.Web.dll"]
