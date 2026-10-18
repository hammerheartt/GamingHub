# ---- build stage: has the .NET SDK, compiles the app ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy only the project file first and restore packages
COPY GamingHub.Api/GamingHub.Api.csproj GamingHub.Api/
RUN dotnet restore GamingHub.Api/GamingHub.Api.csproj

# copy the rest of the code and publish
COPY GamingHub.Api/ GamingHub.Api/
RUN dotnet publish GamingHub.Api/GamingHub.Api.csproj -c Release -o /app/publish

# ---- run stage: only the small runtime, no SDK ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GamingHub.Api.dll"]