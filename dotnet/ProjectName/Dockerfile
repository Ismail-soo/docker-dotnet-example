FROM mcr.microsoft.com/dotnet/core/sdk:2.0-stretch AS build
WORKDIR /src
COPY "ProjectName.sln" .
RUN dotnet restore "ProjectName.sln"
COPY . .
RUN dotnet publish "ProjectName.sln" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:6.0 as final
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish/ ./
ENTRYPOINT ["dotnet", "ProjectName.dll"]
