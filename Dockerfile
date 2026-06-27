FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# csproj primeiro, pra cachear o restore
COPY EmailManager.sln .
COPY src/EmailManager.Domain/*.csproj         src/EmailManager.Domain/
COPY src/EmailManager.Application/*.csproj     src/EmailManager.Application/
COPY src/EmailManager.Infrastructure/*.csproj  src/EmailManager.Infrastructure/
COPY src/EmailManager.Api/*.csproj             src/EmailManager.Api/
RUN dotnet restore src/EmailManager.Api/EmailManager.Api.csproj

COPY . .
RUN dotnet publish src/EmailManager.Api/EmailManager.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmailManager.Api.dll"]